using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ADVance.Command
{
    public class ShowTextCommand : CommandBase
    {
        public override string CommandName => "ShowText";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            await UniTask.Yield();
        }
    }
}