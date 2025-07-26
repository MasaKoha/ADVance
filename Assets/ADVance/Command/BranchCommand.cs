using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class BranchCommand : CommandBase
    {
        private readonly ScenarioBranchRegistry _branches;
        public override string CommandName => "Branch";
        private Subject<(string op, List<string> args, bool result)> _onBranchEvaluated;
        public Observable<(string op, List<string> args, bool result)> OnBranchEvaluated => _onBranchEvaluated ??= new Subject<(string op, List<string> args, bool result)>();

        public BranchCommand(ScenarioBranchRegistry branches)
        {
            _branches = branches;
        }

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var op = args.FirstOrDefault();
            var evaluationArgs = args.Skip(1).ToList();
            var resolvedArgs = Manager.ResolveVariables(evaluationArgs);
            var result = _branches.Evaluate(op, resolvedArgs);

            _onBranchEvaluated?.OnNext((op, evaluationArgs, result));
            UnityEngine.Debug.Log($"Branch: {op} with args [{string.Join(", ", evaluationArgs)}] = {result}");

            // 分岐先IDを決定
            var nextId = Manager.GetNextLineId(result ? 0 : 1);
            Manager.SetCurrentId(nextId);
            await Manager.Wait();
        }
    }
}