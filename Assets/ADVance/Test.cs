using System;
using System.Collections.Generic;
using System.Linq;
using ADVance;
using ADVance.Command.Interface;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance
{
    [Serializable]
    public class ScenarioLine
    {
        public int ID;
        public List<int> NextIDs;
        public string CommandName;
        public List<string> Args;
    }

    [CreateAssetMenu(menuName = "Scenario/ScenarioData")]
    public class ScenarioData : ScriptableObject
    {
        public List<ScenarioLine> Lines;
    }

    public class ScenarioCommandRegistry
    {
        private readonly Dictionary<string, IScenarioCommandAsync> _commands = new();

        public void Register(IScenarioCommandAsync command)
        {
            _commands[command.CommandName] = command;
        }

        public async UniTask<bool> ExecuteAsync(string name, List<string> args)
        {
            if (!_commands.TryGetValue(name, out var cmd))
            {
                return false;
            }

            await cmd.ExecuteCommandAsync(args);
            return true;
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
            return false;
        }
    }
}