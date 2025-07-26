using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ADVance.Command.Operator
{
    public class EqualCommand : CommandBase
    {
        public override string CommandName => "Equal";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            await UniTask.Yield();
            bool result = args.Count >= 2 && args[0] == args[1];
            // 判定結果の利用はエンジン側で
        }
    }
}