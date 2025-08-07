using System.Collections.Generic;
using ADVance.Command.Interface;

namespace ADVance.Command.Operator
{
    public class LessOrEqualCommand : IScenarioBranchEvaluator
    {
        public string OperatorName => "LessOrEqual";

        public bool Evaluate(List<string> args)
        {
            if (args.Count >= 2 && float.TryParse(args[0], out var a) && float.TryParse(args[1], out var b))
            {
                return a <= b;
            }

            return false;
        }
    }
}