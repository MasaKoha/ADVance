using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class LoadAllAssetCommand : CommandBase
    {
        public override string CommandName => "LoadAll";
        private Subject<Unit> _onCompleteLoadAllAssets;
        public Observable<Unit> OnCompleteLoadAllAssets => _onCompleteLoadAllAssets ??= new Subject<Unit>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            await Manager.LoadAllAsset();
            _onCompleteLoadAllAssets?.OnNext(Unit.Default);
            Manager.SetNextLineId();
        }
    }
}