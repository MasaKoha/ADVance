using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace ADVance.AssetLoader
{
    public abstract class AssetLoaderBase : IDisposable
    {
        private readonly List<(string key, string assetPath)> _reservedLoads = new();
        private readonly List<(string key, Object asset)> _loadedAssets = new();

        public void ReserveAssetLoad(string key, string assetPath)
        {
            _reservedLoads.Add((key, assetPath));
        }

        public async UniTask LoadAllAssets()
        {
            var loadTasks = Enumerable
                .Select(_reservedLoads, LoadAssetAsyncInternal)
                .ToList();
            await loadTasks;
        }

        private async UniTask LoadAssetAsyncInternal((string key, string assetPath) assetInfo)
        {
            var path = assetInfo.assetPath;
            var key = assetInfo.key;
            var asset = await LoadAssetAsync<Object>(path);
            _loadedAssets.Add((key, asset));
        }

        public T GetAsset<T>(string key) where T : Object
        {
            var asset = _loadedAssets.Find(a => a.key == key).asset;
            return asset as T;
        }

        public abstract UniTask<T> LoadAssetAsync<T>(string path) where T : Object;

        public void Clear()
        {
            _reservedLoads.Clear();
            _loadedAssets.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}