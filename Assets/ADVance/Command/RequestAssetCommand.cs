using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class RequestAssetCommand : CommandBase
    {
        public override string CommandName => "RequestAsset";
        private Subject<RequestAssetParameter> _onReserveLoadAsset;
        public Observable<RequestAssetParameter> OnReserveLoadAsset => _onReserveLoadAsset ??= new Subject<RequestAssetParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var assetPath = args[1];
            var parameter = new RequestAssetParameter
            {
                Key = key,
                AssetPath = assetPath
            };
            _onReserveLoadAsset.OnNext(parameter);
            Manager.SetNextLineId();
            await UniTask.CompletedTask;
        }
    }

    public struct RequestAssetParameter
    {
        public string Key;
        public string AssetPath;
    }
}