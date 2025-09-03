using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class InitVariableCommand : CommandBase
    {
        public override string CommandName => "InitVariable";
        private Subject<(string varName, object value)> _onVariableInit;
        public Observable<(string varName, object value)> OnVariableInit => _onVariableInit ??= new Subject<(string varName, object value)>();

        public void SetVariable(string varName, object value)
        {
            _onVariableInit?.OnNext((varName, value));
        }

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var varName = args[0];
            var valueString = args[1];

            // 数値として解析を試行
            object value;
            if (int.TryParse(valueString, out var intValue))
            {
                value = intValue;
            }
            else if (float.TryParse(valueString, out var floatValue))
            {
                value = floatValue;
            }
            else if (bool.TryParse(valueString, out var boolValue))
            {
                value = boolValue;
            }
            else
            {
                value = valueString;
            }

            _onVariableInit?.OnNext((varName, value));
            
            // 次のIDに進む
            var nextId = Manager.GetNextLineId();
            Manager.SetCurrentId(nextId);
            await Manager.Wait();
        }
    }
}