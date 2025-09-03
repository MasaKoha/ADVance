using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command.Operator
{
    public class IfLessEqualCommand : CommandBase
    {
        private readonly ScenarioBranchRegistry _branches;
        public override string CommandName => "IfLessEqual";
        private Subject<(List<string> args, bool result)> _onBranchEvaluated;
        public Observable<(List<string> args, bool result)> OnBranchEvaluated => _onBranchEvaluated ??= new Subject<(List<string> args, bool result)>();

        public IfLessEqualCommand(ScenarioBranchRegistry branches)
        {
            _branches = branches;
        }

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var result = false;
            if (args.Count >= 2)
            {
                var resolvedArgs = Manager.ResolveVariables(args);
                result = _branches.Evaluate("LessOrEqual", resolvedArgs);
            }

            _onBranchEvaluated?.OnNext((args, result));

            // 分岐先IDを決定
            var nextId = Manager.GetNextLineId(result ? 0 : 1);
            Manager.SetCurrentId(nextId);
            await Manager.Wait();
        }
    }
}