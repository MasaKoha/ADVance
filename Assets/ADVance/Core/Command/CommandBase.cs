using System.Collections.Generic;
using ADVance.Base;
using ADVance.Command.Interface;
using Cysharp.Threading.Tasks;

namespace ADVance.Command
{
    public abstract class CommandBase : IScenarioCommandAsync
    {
        protected ADVanceManagerBase Manager { get; private set; }
        public abstract string CommandName { get; }
        public abstract UniTask ExecuteCommandAsync(List<string> args);

        public virtual void SetManager(ADVanceManagerBase manager)
        {
            Manager = manager;
        }
    }
}