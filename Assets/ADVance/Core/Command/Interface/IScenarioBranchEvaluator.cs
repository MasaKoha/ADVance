using System.Collections.Generic;

namespace ADVance.Command.Interface
{
    public interface IScenarioBranchEvaluator
    {
        string OperatorName { get; }
        bool Evaluate(List<string> args);
    }
}