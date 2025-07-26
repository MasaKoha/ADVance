using System.Collections.Generic;
using System.Linq;
using ADVance.Command;
using ADVance.Command.Interface;
using ADVance.Command.Operator;
using ADVance.Data;
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
        public readonly Dictionary<string, object> Variables = new();
        public bool IsWaitingForInput { get; private set; } = false;
        protected readonly ScenarioAssetRegistry _assetRegistry = new();
        public ScenarioCommandRegistry CommandRegistry => _commands;
        public ScenarioBranchRegistry BranchRegistry => _branches;
        public ScenarioAssetRegistry AssetRegistry => _assetRegistry;
        private readonly Subject<ScenarioLine> _onLineExecuted = new();
        public Observable<ScenarioLine> OnLineExecuted => _onLineExecuted;
        private readonly Subject<(int choiceIndex, string label)> _onChoiceSelected = new();
        public Observable<(int choiceIndex, string label)> OnChoiceSelected => _onChoiceSelected;
        private readonly Subject<ScenarioAssetRegistry> _onAssetRegistryUpdated = new();
        public Subject<ScenarioAssetRegistry> OnAssetRegistryUpdated => _onAssetRegistryUpdated;
        private readonly Subject<Unit> _onScenarioEnded = new();
        public Observable<Unit> OnScenarioEnded => _onScenarioEnded;

        public void Initialize()
        {
            RegisterCommands();
            OnInitialize();
        }

        protected abstract void OnInitialize();

        protected virtual void RegisterCommands()
        {
            RegisterCommand(new InitVariableCommand());
            RegisterCommand(new ShowTextCommand());
            RegisterCommand(new ChoiceCommand());
            RegisterCommand(new BranchCommand(BranchRegistry));
            RegisterCommand(new IfGreaterEqualCommand(BranchRegistry));
            RegisterCommand(new SetCommand());
            RegisterCommand(new PrintCommand());
            RegisterCommand(new PreloadAssetCommand());
            RegisterBranch(new EqualCommand());
            RegisterBranch(new NotEqualCommand());
            RegisterBranch(new GreaterCommand());
            RegisterBranch(new GreaterOrEqualCommand());
            RegisterBranch(new LessCommand());
            RegisterBranch(new LessOrEqualCommand());
        }

        protected ScenarioLine GetCurrentLine()
        {
            return _lines.GetValueOrDefault(_currentId);
        }

        private int GetNextId(ScenarioLine line, int? branchIndex = null)
        {
            if (line.NextIDs is not { Count: > 0 })
            {
                return line.ID + 1;
            }

            var index = branchIndex ?? 0;
            if (index >= line.NextIDs.Count)
            {
                return line.ID + 1;
            }

            var nextId = line.NextIDs[index];
            if (nextId > 0)
            {
                return nextId;
            }

            // NextIDsがない、または無効な場合は自動的に次の行(ID+1)へ
            return line.ID + 1;
        }

        public List<string> ResolveVariables(List<string> args)
        {
            return args.Select(arg => Variables.TryGetValue(arg, out var variable) ? variable.ToString() : arg).ToList();
        }

        public void SetWaitingForInput(bool waiting)
        {
            IsWaitingForInput = waiting;
        }

        public void ContinueScenario()
        {
            if (!IsWaitingForInput)
            {
                return;
            }

            IsWaitingForInput = false;
        }

        public void RegisterCommand(IScenarioCommandAsync command)
        {
            command.SetManager(this);
            _commands.Register(command);
        }

        public void RegisterBranch(IScenarioBranchEvaluator evaluator)
        {
            _branches.Register(evaluator);
        }

        public async UniTask Wait()
        {
            IsWaitingForInput = false;
            await ProceedAsync();
        }

        public void Load(List<ScenarioLine> lines, int startId = 1)
        {
            _lines = lines.ToDictionary(x => x.ID);
            _currentId = startId;
            ProceedAsync().Forget();
        }

        public virtual void Select(int choiceIndex)
        {
            var current = _lines[_currentId];
            if (current.CommandName != "Choice")
            {
                return;
            }

            var choices = current.Args.Skip(1).ToList();
            _onChoiceSelected.OnNext((choiceIndex, choices[choiceIndex]));
            _currentId = GetNextId(current, choiceIndex);

            // 入力待ちを解除してChoiceCommand内のWait()を継続させる
            IsWaitingForInput = false;
        }

        private async UniTask ProceedAsync()
        {
            while (_lines.TryGetValue(_currentId, out var line))
            {
                _onLineExecuted.OnNext(line);
                await _commands.ExecuteAsync(line.CommandName, line.Args);
                await UniTask.Yield();
            }

            _onScenarioEnded.OnNext(Unit.Default);
        }

        public void SetCurrentId(int id)
        {
            _currentId = id;
        }

        public ScenarioLine GetCurrentScenarioLine()
        {
            return GetCurrentLine();
        }

        public int GetNextLineId(int? branchIndex = null)
        {
            var line = GetCurrentLine();
            return GetNextId(line, branchIndex);
        }
    }
}