using System;
using System.Collections.Generic;
using System.Linq;
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

    public interface IScenarioCommandAsync
    {
        string Name { get; }
        UniTask ExecuteAsync(List<string> args, ScenarioVariables variables);
    }

    public interface IScenarioBranchEvaluator
    {
        string OperatorName { get; }
        bool Evaluate(List<string> args, ScenarioVariables variables);
    }

    public class ScenarioCommandRegistry
    {
        private readonly Dictionary<string, IScenarioCommandAsync> _commands = new();

        public void Register(IScenarioCommandAsync command)
        {
            _commands[command.Name] = command;
        }

        public async UniTask<bool> ExecuteAsync(string name, List<string> args, ScenarioVariables variables)
        {
            if (_commands.TryGetValue(name, out var cmd))
            {
                await cmd.ExecuteAsync(args, variables);
                return true;
            }

            return false;
        }
    }

    public class ScenarioBranchRegistry
    {
        private readonly Dictionary<string, IScenarioBranchEvaluator> _evaluators = new();

        public void Register(IScenarioBranchEvaluator evaluator)
        {
            _evaluators[evaluator.OperatorName] = evaluator;
        }

        public bool Evaluate(string op, List<string> args, ScenarioVariables variables)
        {
            return _evaluators.TryGetValue(op, out var ev) && ev.Evaluate(args, variables);
        }
    }

    public class ScenarioVariables
    {
        private Dictionary<string, string> _strVars = new();
        private Dictionary<string, int> _intVars = new();

        public Subject<(string key, string value)> OnStringChanged { get; } = new();
        public Subject<(string key, int value)> OnIntChanged { get; } = new();

        public void Set(string key, string value)
        {
            _strVars[key] = value;
            OnStringChanged.OnNext((key, value));
        }

        public string Get(string key) => _strVars.TryGetValue(key, out var val) ? val : "";

        public void SetInt(string key, int value)
        {
            _intVars[key] = value;
            OnIntChanged.OnNext((key, value));
        }

        public void Add(string key, int value)
        {
            if (_intVars.ContainsKey(key)) _intVars[key] += value;
            else _intVars[key] = value;
            OnIntChanged.OnNext((key, _intVars[key]));
        }

        public int GetInt(string key) => _intVars.TryGetValue(key, out var val) ? val : 0;

        public string ReplaceVariables(string text)
        {
            foreach (var pair in _strVars)
                text = text.Replace($"${{{pair.Key}}}", pair.Value);
            foreach (var pair in _intVars)
                text = text.Replace($"${{{pair.Key}}}", pair.Value.ToString());
            return text;
        }
    }

    public class ScenarioEngine
    {
        private Dictionary<int, ScenarioLine> _lines;
        private ScenarioVariables _variables = new();
        private ScenarioCommandRegistry _commands = new();
        private ScenarioBranchRegistry _branches = new();
        private int _currentId;

        public Subject<ScenarioLine> OnLineExecuted { get; } = new();
        public Subject<(int choiceIndex, string label)> OnChoiceSelected { get; } = new();

        public ScenarioVariables Variables => _variables;
        public ScenarioCommandRegistry CommandRegistry => _commands;
        public ScenarioBranchRegistry BranchRegistry => _branches;

        public void RegisterCommand(IScenarioCommandAsync command) => _commands.Register(command);
        public void RegisterBranch(IScenarioBranchEvaluator evaluator) => _branches.Register(evaluator);

        public ScenarioEngine()
        {
            RegisterCommand(new ShowTextCommandSO());
            RegisterCommand(new ChoiceCommandSO());
            RegisterCommand(new BranchCommandSO(BranchRegistry));
        }

        public void Load(List<ScenarioLine> lines, int startId = 1)
        {
            _lines = lines.ToDictionary(x => x.ID);
            _currentId = startId;
            ProceedAsync().Forget();
        }

        public void Select(int choiceIndex)
        {
            var current = _lines[_currentId];
            if (current.CommandName == "Choice")
            {
                var varName = current.Args.FirstOrDefault();
                var choices = current.Args.Skip(1).ToList();
                if (!string.IsNullOrEmpty(varName))
                    _variables.Set(varName, choices[choiceIndex]);
                OnChoiceSelected.OnNext((choiceIndex, choices[choiceIndex]));
                _currentId = current.NextIDs[choiceIndex];
                ProceedAsync().Forget();
            }
        }

// Showコマンド
        public class ShowTextCommandSO : ScenarioCommandSO
        {
            public override string Name => "Show";

            public override async UniTask ExecuteAsync(List<string> args, ScenarioVariables variables)
            {
                Debug.Log(variables.ReplaceVariables(string.Join(" ", args)));
                await UniTask.Yield();
            }
        } // Choiceコマンド

        public abstract class ScenarioCommandSO : IScenarioCommandAsync
        {
            public abstract string Name { get; }
            public abstract UniTask ExecuteAsync(List<string> args, ScenarioVariables variables);
        }


        public class ChoiceCommandSO : ScenarioCommandSO
        {
            public override string Name => "Choice";
            public Subject<(int choiceIndex, string label)> OnChoiceSelected { get; } = new();

            public override async UniTask ExecuteAsync(List<string> args, ScenarioVariables variables)
            {
                var varName = args.FirstOrDefault();
                var choices = args.Skip(1).ToList();
                for (int i = 0; i < choices.Count; i++)
                    Debug.Log($"{i + 1}: {choices[i]}");
                // 選択待ちの処理は別途
                await UniTask.Yield();
            }
        } // Branchコマンド

        public class BranchCommandSO : ScenarioCommandSO
        {
            private ScenarioBranchRegistry _branches;
            public BranchCommandSO(ScenarioBranchRegistry branches) => _branches = branches;
            public override string Name => "Branch";

            public override async UniTask ExecuteAsync(List<string> args, ScenarioVariables variables)
            {
                string op = args.FirstOrDefault();
                bool result = _branches.Evaluate(op, args.Skip(1).ToList(), variables);
                // 分岐先のID選択はエンジン側で制御
                await UniTask.Yield();
            }
        }

        private async UniTaskVoid ProceedAsync()
        {
            while (_lines.TryGetValue(_currentId, out var line))
            {
                OnLineExecuted.OnNext(line);

                // コマンド実行
                if (!await _commands.ExecuteAsync(line.CommandName, line.Args, _variables))
                {
                    Debug.LogWarning($"未知のコマンド: {line.CommandName}");
                }

                // Choiceコマンドは選択待ちなのでループを抜ける
                if (line.CommandName == "Choice")
                    return;

                // Branchコマンドは分岐先IDを決定
                if (line.CommandName == "Branch")
                {
                    string op = line.Args.FirstOrDefault();
                    bool result = _branches.Evaluate(op, line.Args.Skip(1).ToList(), _variables);
                    _currentId = result ? line.NextIDs[0] : line.NextIDs[1];
                }
                else
                {
                    // 通常は次のIDへ
                    _currentId = line.NextIDs.FirstOrDefault();
                }

                await UniTask.Yield();
            }

            Debug.Log("シナリオ終了");
        }
    }

    public class ChoiceCommandSO : ScenarioEngine.ScenarioCommandSO
    {
        public override string Name => "Choice";
        public Subject<(int choiceIndex, string label)> OnChoiceSelected { get; } = new();

        public override async UniTask ExecuteAsync(List<string> args, ScenarioVariables variables)
        {
            var varName = args.FirstOrDefault();
            var choices = args.Skip(1).ToList();
            for (int i = 0; i < choices.Count; i++)
                Debug.Log($"{i + 1}: {choices[i]}");
            // 選択待ちはエンジン側で
            await UniTask.Yield();
        }
    }

    public class BranchCommandSO : ScenarioEngine.ScenarioCommandSO
    {
        private ScenarioBranchRegistry _branches;
        public BranchCommandSO(ScenarioBranchRegistry branches) => _branches = branches;
        public override string Name => "Branch";

        public override async UniTask ExecuteAsync(List<string> args, ScenarioVariables variables)
        {
            // 分岐判定のみ行う
            // 分岐判定のみ実行（遷移は ProceedAsync で制御）
            string op = args.FirstOrDefault();
            _branches.Evaluate(op, args.Skip(1).ToList(), variables);
            await UniTask.Yield();
        }
    }
}