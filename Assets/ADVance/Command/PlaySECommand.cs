using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class PlaySECommand : CommandBase
    {
        public override string CommandName => "PlaySE";

        private Subject<string> _onPlaySe;
        public Observable<string> OnPlaySe => _onPlaySe ??= new Subject<string>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var bgmPath = args[0];
            _onPlaySe?.OnNext(bgmPath);
            return UniTask.CompletedTask;
        }
    }
}