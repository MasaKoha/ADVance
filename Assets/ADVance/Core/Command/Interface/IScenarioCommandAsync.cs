using System.Collections.Generic;
using ADVance.Base;
using Cysharp.Threading.Tasks;

namespace ADVance.Command.Interface
{
    public interface IScenarioCommandAsync
    {
        public string CommandName { get; }
        public UniTask ExecuteCommandAsync(List<string> args);
        public void SetManager(ADVanceManagerBase manager);
    }
}