using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class PreloadAssetCommand : CommandBase
    {
        public override string CommandName => "PreloadAsset";
        private Subject<string> _onAssetRegistered;
        public Observable<string> OnAssetRegistered => _onAssetRegistered ??= new Subject<string>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var assetPath = args[0];
            _onAssetRegistered.OnNext(assetPath);
            await UniTask.CompletedTask;
        }
    }
}