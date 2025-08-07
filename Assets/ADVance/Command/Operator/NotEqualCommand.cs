using System.Collections.Generic;
using ADVance.Command.Interface;

namespace ADVance.Command.Operator
{
    public class NotEqualCommand : IScenarioBranchEvaluator
    {
        public string OperatorName => "NotEqual";

        public bool Evaluate(List<string> args)
        {
            if (args.Count >= 2)
            {
                return args[0] != args[1];
            }

            return false;
        }
    }
}