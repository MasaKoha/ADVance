using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class FinishCommand : CommandBase
    {
        public override string CommandName => "Finish";
        
        private Subject<Unit> _onFinish;
        public Observable<Unit> OnFinish => _onFinish ??= new Subject<Unit>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            _onFinish?.OnNext(Unit.Default);
            Manager.Stop();
            await UniTask.CompletedTask;
        }
    }
}