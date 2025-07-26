using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ADVance.Command.Operator
{
    public class NotEqualCommand : CommandBase
    {
        public override string CommandName => "NotEqual";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            await UniTask.Yield();
            bool result = args.Count >= 2 && args[0] != args[1];
        }
    }
}