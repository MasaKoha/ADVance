using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ADVance.AssetLoader
{
    public abstract class AssetLoaderBase : IDisposable
    {
        private readonly List<(string key, string assetPath)> _requestAssets = new();
        private readonly List<(string key, Object asset)> _loadedAssets = new();

        private readonly List<(string key, string assetPath)> _requestSpriteAssets = new();
        private readonly List<(string key, Sprite asset)> _loadedSpriteAssets = new();

        public void RequestAssetLoad(string key, string assetPath)
        {
            _requestAssets.Add((key, assetPath));
        }

        public void RequestSpriteAsset(string key, string assetPath)
        {
            _requestSpriteAssets.Add((key, assetPath));
        }

        public async UniTask LoadAllAssets()
        {
            var spriteLoadTasks = Enumerable
                .Select(_requestSpriteAssets, LoadSpriteAsyncInternal)
                .ToList();
            var loadTasks = Enumerable
                .Select(_requestAssets, LoadAssetAsyncInternal)
                .ToList();
            await loadTasks.Concat(spriteLoadTasks);
        }

        private async UniTask LoadAssetAsyncInternal((string key, string assetPath) assetInfo)
        {
            var path = assetInfo.assetPath;
            var key = assetInfo.key;
            var asset = await LoadAssetAsync<Object>(path);
            _loadedAssets.Add((key, asset));
        }

        /// <summary>
        /// Sprite専用のロード処理
        /// Object で読み込んでしまうと Texture2D として読み込まれてしまうため
        /// </summary>
        private async UniTask LoadSpriteAsyncInternal((string key, string assetPath) assetInfo)
        {
            var path = assetInfo.assetPath;
            var key = assetInfo.key;
            var asset = await LoadSpriteAsync(path);
            _loadedSpriteAssets.Add((key, asset));
        }

        public T GetAsset<T>(string key) where T : Object
        {
            return (T)_loadedAssets.Find(asset => asset.key == key).asset;
        }

        /// <summary>
        /// スプライト専用の取得処理
        /// </summary>
        public Sprite GetSpriteAsset(string key)
        {
            return _loadedSpriteAssets.Find(asset => asset.key == key).asset;
        }

        protected abstract UniTask<T> LoadAssetAsync<T>(string path) where T : Object;
        protected abstract UniTask<Sprite> LoadSpriteAsync(string path);

        public void Clear()
        {
            _requestAssets.Clear();
            _loadedAssets.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}