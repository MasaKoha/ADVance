using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class PrintCommand : CommandBase
    {
        public override string CommandName => "Print";
        private Subject<string> _onPrint;
        public Observable<string> OnPrint => _onPrint ??= new Subject<string>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var varName = args[0];
            _onPrint?.OnNext(varName);
            // 次のIDに進む
            var nextId = Manager.GetNextLineId();
            Manager.SetCurrentId(nextId);
            return UniTask.CompletedTask;
        }
    }
}