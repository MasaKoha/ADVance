using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class RequestSoundCommand : CommandBase
    {
        public override string CommandName => "RequestSound";

        private Subject<RequestSoundParameter> _onRequestSound;
        public Observable<RequestSoundParameter> OnRequestSound => _onRequestSound ??= new Subject<RequestSoundParameter>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var assetPath = args.Count > 1 ? args[1] : "";
            _onRequestSound?.OnNext(new RequestSoundParameter()
            {
                Key = key,
                AssetPath = assetPath,
            });
            return UniTask.CompletedTask;
        }
    }

    public struct RequestSoundParameter
    {
        public string Key;
        public string AssetPath;
    }
}