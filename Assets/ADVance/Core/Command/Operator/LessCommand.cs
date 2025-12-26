using System.Collections.Generic;
using ADVance.Command.Interface;
using ADVance.Utility;

namespace ADVance.Command.Operator
{
    public class LessCommand : IScenarioBranchEvaluator
    {
        public string OperatorName => "Less";

        public bool Evaluate(List<string> args)
        {
            if (args.Count < 2)
            {
                return false;
            }

            var a = args[0].ToFloat(float.NaN);
            if (float.IsNaN(a) && !IsNaNLiteral(args[0]))
            {
                return false;
            }

            var b = args[1].ToFloat(float.NaN);
            if (float.IsNaN(b) && !IsNaNLiteral(args[1]))
            {
                return false;
            }

            return a < b;
        }

        private static bool IsNaNLiteral(string value)
        {
            return string.Equals(value?.Trim(), "NaN", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
