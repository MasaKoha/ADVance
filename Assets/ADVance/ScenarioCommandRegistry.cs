using System.Collections.Generic;
using ADVance.Command.Interface;
using Cysharp.Threading.Tasks;

namespace ADVance
{
    public class ScenarioCommandRegistry
    {
        private readonly Dictionary<string, IScenarioCommandAsync> _commands = new();

        public void Register(IScenarioCommandAsync command)
        {
            _commands[command.CommandName] = command;
        }

        public async UniTask<bool> ExecuteAsync(string commandName, List<string> args)
        {
            if (!_commands.TryGetValue(commandName, out var command))
            {
                return false;
            }

            await command.ExecuteCommandAsync(args);
            return true;
        }

        public IScenarioCommandAsync GetCommand(string commandName)
        {
            return _commands.GetValueOrDefault(commandName);
        }

        public T GetCommand<T>() where T : class, IScenarioCommandAsync
        {
            foreach (var command in _commands.Values)
            {
                if (command is T typedCommand)
                {
                    return typedCommand;
                }
            }
            return null;
        }
    }

    public class ScenarioBranchRegistry
    {
        private readonly Dictionary<string, IScenarioBranchEvaluator> _evaluators = new();

        public void Register(IScenarioBranchEvaluator evaluator)
        {
            _evaluators[evaluator.OperatorName] = evaluator;
        }

        public bool Evaluate(string op, List<string> args)
        {
            return _evaluators.TryGetValue(op, out var evaluator) && evaluator.Evaluate(args);
        }

        public IScenarioBranchEvaluator GetEvaluator(string operatorName)
        {
            return _evaluators.GetValueOrDefault(operatorName);
        }
    }
}