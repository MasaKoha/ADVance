using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class AddCommand : CommandBase
    {
        public override string CommandName => "Add";
        private Subject<(string varName, object oldValue, object newValue)> _onVariableAdded;
        public Observable<(string varName, object oldValue, object newValue)> OnVariableAdded => _onVariableAdded ??= new Subject<(string varName, object oldValue, object newValue)>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            if (args.Count < 2)
            {
                Manager.SetCurrentId(Manager.GetNextLineId());
                return UniTask.CompletedTask;
            }

            var varName = args[0];
            var addValueString = args[1];

            // 加算する値を解析
            var addValue = addValueString.ToFloat(float.NaN);
            if (float.IsNaN(addValue) && !string.Equals(addValueString?.Trim(), "NaN", System.StringComparison.OrdinalIgnoreCase))
            {
                Manager.SetCurrentId(Manager.GetNextLineId());
                return UniTask.CompletedTask;
            }

            // 既存の変数値を取得
            var oldValue = Manager.Variables.TryGetValue(varName, out var currentValue) ? currentValue : 0;

            // 現在の値を数値として解析
            var currentNumValue = currentValue.ToFloat(0f);

            // 新しい値を計算
            var newValue = currentNumValue + addValue;

            // 整数として保存するか小数として保存するかを判定
            object finalValue;
            if (newValue == (int)newValue)
            {
                finalValue = (int)newValue;
            }
            else
            {
                finalValue = newValue;
            }

            // 変数を更新
            Manager.SetVariable(varName, finalValue);
            _onVariableAdded?.OnNext((varName, oldValue, finalValue));
            Manager.SetCurrentId(Manager.GetNextLineId());
            return UniTask.CompletedTask;
        }
    }
}
