using System.Collections.Generic;
using System.Linq;
using ADVance.Command;
using ADVance.Command.Interface;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Base
{
    public abstract class ADVanceManagerBase : MonoBehaviour
    {
        private Dictionary<int, ScenarioLine> _lines;
        private readonly ScenarioCommandRegistry _commands = new();
        private readonly ScenarioBranchRegistry _branches = new();
        private int _currentId;

        public Subject<ScenarioLine> OnLineExecuted { get; } = new();
        public Subject<(int choiceIndex, string label)> OnChoiceSelected { get; } = new();

        public ScenarioCommandRegistry CommandRegistry => _commands;
        public ScenarioBranchRegistry BranchRegistry => _branches;

        public void RegisterCommand(IScenarioCommandAsync command) => _commands.Register(command);
        public void RegisterBranch(IScenarioBranchEvaluator evaluator) => _branches.Register(evaluator);

        public void Initialize()
        {
            RegisterCommand(new ShowTextCommand());
            RegisterCommand(new ChoiceCommand());
            RegisterCommand(new BranchCommand(BranchRegistry));
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
                OnChoiceSelected.OnNext((choiceIndex, choices[choiceIndex]));
                _currentId = current.NextIDs[choiceIndex];
                ProceedAsync().Forget();
            }
        }

        private async UniTaskVoid ProceedAsync()
        {
            while (_lines.TryGetValue(_currentId, out var line))
            {
                OnLineExecuted.OnNext(line);

                // コマンド実行
                if (!await _commands.ExecuteAsync(line.CommandName, line.Args))
                {
                    Debug.LogWarning($"未知のコマンド: {line.CommandName}");
                }

                // Choiceコマンドは選択待ちなのでループを抜ける
                if (line.CommandName == "Choice")
                    return;

                // Branchコマンドは分岐先IDを決定
                if (line.CommandName == "Branch")
                {
                    var op = line.Args.FirstOrDefault();
                    var result = _branches.Evaluate(op, line.Args.Skip(1).ToList());
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
}