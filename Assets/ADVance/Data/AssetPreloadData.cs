using System;
using System.Collections.Generic;
using ADVance.Manager;
using ADVance.Manager.Interface;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.Data
{
    public class AssetPreloadData
    {
        public string AssetPath;

        // バイト単位
        public long EstimatedSize;
        public bool IsDownloaded;
        public UnityEngine.Object CachedAsset;
    }

    [Serializable]
    public class ScenarioAssetRegistry
    {
        public List<AssetPreloadData> PreloadAssets = new();
        public long TotalEstimatedSize => CalculateTotalSize();

        public void AddAsset(string assetPath)
        {
            PreloadAssets.Add(new AssetPreloadData { AssetPath = assetPath, });
        }

        public void ClearAssets()
        {
            PreloadAssets.Clear();
        }

        public async UniTask UpdateAssetSizes(IAssetPreloadManager preloadManager)
        {
            foreach (var asset in PreloadAssets)
            {
                if (asset.EstimatedSize == 0)
                {
                    asset.EstimatedSize = await preloadManager.CalculateAssetSize(asset.AssetPath);
                }
            }
        }

        private long CalculateTotalSize()
        {
            long total = 0;
            foreach (var asset in PreloadAssets)
            {
                total += asset.EstimatedSize;
            }

            return total;
        }
    }
}