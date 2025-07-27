using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class ChoiceCommand : CommandBase
    {
        public override string CommandName => "Choice";
        public Subject<(int choiceIndex, string label)> OnChoiceSelected { get; } = new();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var varName = args.FirstOrDefault();
            var choices = args.Skip(1).ToList();
            await UniTask.Yield();
        }
    }
}