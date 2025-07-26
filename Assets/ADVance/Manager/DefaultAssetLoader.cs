using System.IO;
using ADVance.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.Manager
{
    /// <summary>
    /// デフォルトのアセットローダー実装
    /// Resources.LoadとResources.LoadAsyncを使用
    /// </summary>
    public class DefaultAssetLoader : AssetLoader
    {
        public override async UniTask<(bool success, Object asset)> LoadAssetAsync(AssetPreloadData assetData)
        {
            try
            {
                Debug.Log($"Loading asset with DefaultAssetLoader: {assetData.AssetPath}");

                if (assetData.AssetPath.StartsWith("Resources/"))
                {
                    return await LoadFromResources(assetData);
                }

                return (false, null);
            }
            catch (System.Exception e)
            {
                return (false, null);
            }
        }

        private async UniTask<(bool success, Object asset)> LoadFromResources(AssetPreloadData assetData)
        {
            try
            {
                var resourcePath = assetData.AssetPath["Resources/".Length..];
                resourcePath = Path.ChangeExtension(resourcePath, null); // 拡張子を除去
                var asset = await LoadResourceAsync(resourcePath);
                if (asset != null)
                {
                    return (true, asset);
                }

                Debug.LogWarning($"Asset not found in Resources: {resourcePath}");
                return (false, null);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Resources loading error: {e.Message}");
                return (false, null);
            }
        }

        private async UniTask<Object> LoadResourceAsync(string resourcePath)
        {
            try
            {
                var request = Resources.LoadAsync(resourcePath);
                await request;
                return request.asset;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load asset from {resourcePath}: {e.Message}");
                return null;
            }
        }
    }
}