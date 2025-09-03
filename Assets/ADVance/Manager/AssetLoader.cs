using ADVance.Data;
using ADVance.Manager.Interface;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.Manager
{
    public abstract class AssetLoader : MonoBehaviour, IAssetLoader
    {
        public abstract UniTask<(bool success, Object asset)> LoadAssetAsync(AssetPreloadData assetData);
    }
}