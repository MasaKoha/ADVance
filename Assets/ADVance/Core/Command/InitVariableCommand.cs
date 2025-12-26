using System.Collections.Generic;
using ADVance.Utility;
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
            var intValue = valueString.ToInt(int.MinValue);
            if (intValue != int.MinValue || string.Equals(valueString?.Trim(), int.MinValue.ToString(), System.StringComparison.Ordinal))
            {
                value = intValue;
            }
            else
            {
                var floatValue = valueString.ToFloat(float.NaN);
                if (!float.IsNaN(floatValue) || string.Equals(valueString?.Trim(), "NaN", System.StringComparison.OrdinalIgnoreCase))
                {
                    value = floatValue;
                }
                else if (string.Equals(valueString?.Trim(), "true", System.StringComparison.OrdinalIgnoreCase)
                         || string.Equals(valueString?.Trim(), "false", System.StringComparison.OrdinalIgnoreCase))
                {
                    value = valueString.ToBool();
                }
                else
                {
                    value = valueString;
                }
            }
            _onVariableInit?.OnNext((varName, value));
            await Manager.Wait();
            Manager.SetNextLineId();
        }
    }
}
