using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class BranchCommand : CommandBase
    {
        private ScenarioBranchRegistry _branches;
        public override string CommandName => "Branch";
        private Subject<bool> _onBranchSelected;
        public Observable<bool> OnBranchSelected => _onBranchSelected ??= new Subject<bool>();

        public BranchCommand(ScenarioBranchRegistry branches)
        {
            _branches = branches;
        }

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var op = args.FirstOrDefault();
            var value = _branches.Evaluate(op, args);
            _onBranchSelected?.OnNext(value);
            return UniTask.CompletedTask;
        }
    }
}