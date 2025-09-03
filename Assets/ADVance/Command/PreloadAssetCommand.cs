using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Command
{
    public class PreloadAssetCommand : CommandBase
    {
        public override string CommandName => "PreloadAsset";
        public Subject<(string assetPath, long estimatedSize)> OnAssetRegistered { get; } = new();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            if (args.Count < 1)
            {
                Debug.LogError("PreloadAsset command requires at least 1 argument: [assetPath] [estimatedSize(optional)]");
                return;
            }

            var assetPath = args[0];

            long estimatedSize = 0;
            if (args.Count > 1)
            {
                if (!long.TryParse(args[1], out estimatedSize))
                {
                    Debug.LogWarning($"Invalid size format: {args[1]}. Using 0 as default.");
                    estimatedSize = 0;
                }
            }

            OnAssetRegistered.OnNext((assetPath, estimatedSize));
            Debug.Log($"Asset registered for preload: {assetPath} ({estimatedSize} bytes)");

            await UniTask.CompletedTask;
        }
    }
}