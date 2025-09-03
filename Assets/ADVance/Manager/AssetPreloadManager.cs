using System.Collections.Generic;
using System.IO;
using ADVance.Data;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Manager
{
    public class AssetPreloadManager : MonoBehaviour
    {
        public Subject<float> OnDownloadProgress { get; } = new();
        public Subject<(string assetPath, bool success)> OnAssetDownloaded { get; } = new();
        public Subject<bool> OnAllAssetsDownloaded { get; } = new();
        public Subject<long> OnAssetSizeCalculated { get; } = new();

        private readonly Dictionary<string, Object> _loadedAssets = new();
        private bool _isDownloading = false;

        public bool IsDownloading => _isDownloading;

        public UniTask<long> CalculateAssetSize(string assetPath)
        {
            try
            {
                // Resourcesフォルダ内のアセットのサイズを計算
                if (assetPath.StartsWith("Resources/"))
                {
                    var resourcePath = assetPath.Substring("Resources/".Length);
                    resourcePath = Path.ChangeExtension(resourcePath, null); // 拡張子を除去

                    var asset = Resources.Load(resourcePath);
                    if (asset == null)
                    {
                        Debug.LogWarning($"Asset not found in Resources: {resourcePath}");
                        return UniTask.FromResult<long>(0);
                    }

                    // アセットの型に応じたサイズ推定
                    var estimatedSize = EstimateAssetSize(asset);
                    OnAssetSizeCalculated.OnNext(estimatedSize);
                    return UniTask.FromResult(estimatedSize);
                }

                // StreamingAssetsやその他のパスの場合
                if (assetPath.StartsWith("StreamingAssets/"))
                {
                    var fullPath = Path.Combine(Application.streamingAssetsPath, assetPath.Substring("StreamingAssets/".Length));
                    if (File.Exists(fullPath))
                    {
                        var fileInfo = new FileInfo(fullPath);
                        OnAssetSizeCalculated.OnNext(fileInfo.Length);
                        return UniTask.FromResult(fileInfo.Length);
                    }
                }

                Debug.LogWarning($"Cannot calculate size for asset: {assetPath}");
                return UniTask.FromResult<long>(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error calculating asset size for {assetPath}: {e.Message}");
                return UniTask.FromResult<long>(0);
            }
        }

        private long EstimateAssetSize(Object asset)
        {
            switch (asset)
            {
                case Texture2D texture:
                    return texture.width * texture.height * 4; // RGBA32として計算

                case Sprite sprite:
                    var spriteTexture = sprite.texture;
                    return spriteTexture.width * spriteTexture.height * 4;

                case AudioClip clip:
                    return (long)(clip.length * clip.frequency * clip.channels * 2); // 16bit音声として計算

                default:
                    return 1024 * 1024; // デフォルト1MB
            }
        }

        public async UniTask PreloadAssetsAsync(ScenarioAssetRegistry assetRegistry)
        {
            if (_isDownloading)
            {
                Debug.LogWarning("Asset preloading is already in progress");
                return;
            }

            _isDownloading = true;
            var totalAssets = assetRegistry.PreloadAssets.Count;
            var completedAssets = 0;

            try
            {
                foreach (var assetData in assetRegistry.PreloadAssets)
                {
                    if (assetData.IsDownloaded)
                    {
                        completedAssets++;
                        OnDownloadProgress.OnNext((float)completedAssets / totalAssets);
                        continue;
                    }

                    bool success = await LoadAssetAsync(assetData);
                    assetData.IsDownloaded = success;

                    OnAssetDownloaded.OnNext((assetData.AssetPath, success));

                    completedAssets++;
                    OnDownloadProgress.OnNext((float)completedAssets / totalAssets);

                    // フレーム待機でUI更新を許可
                    await UniTask.Yield();
                }

                OnAllAssetsDownloaded.OnNext(true);
                Debug.Log($"Asset preloading completed. Successfully loaded: {completedAssets}/{totalAssets}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during asset preloading: {e.Message}");
                OnAllAssetsDownloaded.OnNext(false);
            }
            finally
            {
                _isDownloading = false;
            }

            return;
        }

        private UniTask<bool> LoadAssetAsync(AssetPreloadData assetData)
        {
            try
            {
                if (_loadedAssets.ContainsKey(assetData.AssetPath))
                {
                    return UniTask.FromResult(true); // 既に読み込み済み
                }

                // Resourcesフォルダからの読み込み
                if (assetData.AssetPath.StartsWith("Resources/"))
                {
                    var resourcePath = assetData.AssetPath.Substring("Resources/".Length);
                    resourcePath = Path.ChangeExtension(resourcePath, null); // 拡張子を除去

                    var asset = Resources.Load(resourcePath);
                    if (asset != null)
                    {
                        _loadedAssets[assetData.AssetPath] = asset;
                        assetData.CachedAsset = asset;
                        return UniTask.FromResult(true);
                    }
                }

                Debug.LogWarning($"Failed to load asset: {assetData.AssetPath}");
                return UniTask.FromResult(false);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading asset {assetData.AssetPath}: {e.Message}");
                return UniTask.FromResult(false);
            }
        }

        public T GetLoadedAsset<T>(string assetPath) where T : Object
        {
            if (_loadedAssets.TryGetValue(assetPath, out var asset))
            {
                return asset as T;
            }

            return null;
        }

        public void ClearLoadedAssets()
        {
            _loadedAssets.Clear();
        }

        private void OnDestroy()
        {
            OnDownloadProgress?.Dispose();
            OnAssetDownloaded?.Dispose();
            OnAllAssetsDownloaded?.Dispose();
            OnAssetSizeCalculated?.Dispose();
        }
    }
}