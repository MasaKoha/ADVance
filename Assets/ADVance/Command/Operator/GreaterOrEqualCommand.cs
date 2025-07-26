using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ADVance.Command.Operator
{
    public class GreaterOrEqualCommand : CommandBase
    {
        public override string CommandName => "GreaterOrEqual";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            await UniTask.Yield();
            bool result = false;
            if (args.Count >= 2 && float.TryParse(args[0], out var a) && float.TryParse(args[1], out var b))
                result = a >= b;
        }
    }
}