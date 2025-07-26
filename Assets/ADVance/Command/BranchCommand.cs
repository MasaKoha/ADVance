using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace ADVance.Command
{
    public class BranchCommand : CommandBase
    {
        private ScenarioBranchRegistry _branches;
        public BranchCommand(ScenarioBranchRegistry branches) => _branches = branches;
        public override string CommandName => "Branch";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var op = args.FirstOrDefault();
            // 分岐先のID選択はエンジン側で制御
            await UniTask.Yield();
        }
    }
}