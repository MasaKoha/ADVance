using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class SetCommand : CommandBase
    {
        public override string CommandName => "Set";
        private Subject<(string varName, object value)> _onVariableSet;
        public Observable<(string varName, object value)> OnVariableSet => _onVariableSet ??= new Subject<(string varName, object value)>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var varName = args[0];
            var valueString = args[1];
            // 数値として解析を試行
            object value;
            if (int.TryParse(valueString, out var intValue))
            {
                value = intValue;
            }
            else
            {
                value = valueString;
            }

            _onVariableSet?.OnNext((varName, value));
            Manager.SetCurrentId(Manager.GetNextLineId());
            return UniTask.CompletedTask;
        }
    }
}