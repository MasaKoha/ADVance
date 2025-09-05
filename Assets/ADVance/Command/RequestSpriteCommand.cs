using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class RequestSpriteCommand : CommandBase
    {
        public override string CommandName => "RequestSprite";
        private Subject<RequestSpriteAssetParameter> _onRequestLoadSpriteAsset;
        public Observable<RequestSpriteAssetParameter> OnRequestLoadSpriteAsset => _onRequestLoadSpriteAsset ??= new Subject<RequestSpriteAssetParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var assetPath = args[1];
            var parameter = new RequestSpriteAssetParameter
            {
                Key = key,
                AssetPath = assetPath
            };
            _onRequestLoadSpriteAsset.OnNext(parameter);
            Manager.SetNextLineId();
            await UniTask.CompletedTask;
        }
    }

    public struct RequestSpriteAssetParameter
    {
        public string Key;
        public string AssetPath;
    }
}