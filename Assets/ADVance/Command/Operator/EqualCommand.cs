using System.Collections.Generic;
using ADVance.Command.Interface;

namespace ADVance.Command.Operator
{
    public class EqualCommand : IScenarioBranchEvaluator
    {
        public string OperatorName => "Equal";

        public bool Evaluate(List<string> args)
        {
            return args.Count >= 2 && args[0] == args[1];
        }
    }
}