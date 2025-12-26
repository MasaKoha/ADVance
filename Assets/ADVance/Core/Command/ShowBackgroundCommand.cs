using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class ShowBackgroundCommand : CommandBase
    {
        public override string CommandName => "ShowBackground";
        private Subject<string> _onShowBackground;
        public Observable<string> OnShowBackground => _onShowBackground ??= new Subject<string>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var tag = args[0];
            _onShowBackground?.OnNext(tag);
            Manager.SetNextLineId();
            await UniTask.CompletedTask;
        }
    }
}