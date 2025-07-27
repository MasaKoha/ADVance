using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ADVance.Command.Interface
{
    public interface IScenarioCommandAsync
    {
        public string CommandName { get; }
        public UniTask ExecuteCommandAsync(List<string> args);
    }
}