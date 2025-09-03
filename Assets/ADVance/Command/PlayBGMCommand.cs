using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class PlayBGMCommand : CommandBase
    {
        public override string CommandName => "PlayBGM";

        private Subject<string> _onPlayBGM;
        public Observable<string> OnPlayBGM => _onPlayBGM ??= new Subject<string>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var bgmPath = args[0];
            _onPlayBGM?.OnNext(bgmPath);
            return UniTask.CompletedTask;
        }
    }
}