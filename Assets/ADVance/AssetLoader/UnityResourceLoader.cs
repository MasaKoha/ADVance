using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.AssetLoader
{
    /// <summary>
    /// デフォルトのアセットローダー実装
    /// Resources.LoadAsyncを使用
    /// </summary>
    public class UnityResourceLoader : AssetLoaderBase
    {
        public override async UniTask<T> LoadAssetAsync<T>(string path)
        {
            var request = Resources.LoadAsync(path);
            await request;
            var asset = request.asset;
            if (asset != null)
            {
                return asset as T;
            }

            Debug.LogWarning($"Failed to load asset at path: {path}");
            return null;
        }
    }
}