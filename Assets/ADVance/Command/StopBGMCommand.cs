using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    namespace ADVance.Command
    {
        public class StopBGMCommand : CommandBase
        {
            public override string CommandName => "StopBGM";

            private Subject<Unit> _onStopBGM;
            public Observable<Unit> OnStopBGM => _onStopBGM ??= new Subject<Unit>();

            public override UniTask ExecuteCommandAsync(List<string> args)
            {
                _onStopBGM.OnNext(Unit.Default);
                return UniTask.CompletedTask;
            }
        }
    }
}