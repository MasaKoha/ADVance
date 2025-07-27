using System.Collections.Generic;
using ADVance.Command.Interface;
using Cysharp.Threading.Tasks;

namespace ADVance.Command
{
    public abstract class CommandBase : IScenarioCommandAsync
    {
        public abstract string CommandName { get; }
        public abstract UniTask ExecuteCommandAsync(List<string> args);
    }
}