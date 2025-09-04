using ADVance.Data;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Manager.Interface
{
    public interface IAssetPreloadManager
    {
        Subject<float> OnDownloadProgress { get; }
        Subject<(string assetPath, bool success)> OnAssetDownloaded { get; }
        Subject<bool> OnAllAssetsDownloaded { get; }
        Subject<long> OnAssetSizeCalculated { get; }

        bool IsDownloading { get; }

        UniTask<long> CalculateAssetSize(string assetPath);
        UniTask PreloadAssetsAsync(ScenarioAssetRegistry assetRegistry);
        T GetLoadedAsset<T>(string assetPath) where T : Object;
        void ClearLoadedAssets();
    }
}