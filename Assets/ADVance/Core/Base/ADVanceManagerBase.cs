using System;
using System.Collections.Generic;
using System.Linq;
using ADVance.AssetLoader;
using ADVance.Command;
using ADVance.Command.Interface;
using ADVance.Command.Operator;
using ADVance.Data;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Base
{
    public sealed class ScenarioRuntimeContext
    {
        public Dictionary<int, ScenarioLine> Lines;
        public int ResumeId;
    }

    public abstract class ADVanceManagerBase : MonoBehaviour
    {
        private Dictionary<int, ScenarioLine> _lines;

        private readonly Stack<ScenarioRuntimeContext> _scenarioContexts = new();
        private readonly ScenarioCommandRegistry _commands = new();
        private readonly ScenarioBranchRegistry _branches = new();
        private int _currentId;
        private readonly Dictionary<string, object> _variables = new();
        public IReadOnlyDictionary<string, object> Variables => _variables;
        public bool IsWaitingForInput { get; private set; } = false;
        private bool _isScenarioEnded = false;
        private bool _hasDispatchedScenarioEnd = false;
        protected readonly ScenarioAssetRegistry _assetRegistry = new();
        public ScenarioCommandRegistry CommandRegistry => _commands;
        public ScenarioBranchRegistry BranchRegistry => _branches;
        public ScenarioAssetRegistry AssetRegistry => _assetRegistry;
        private readonly Subject<ScenarioLine> _onLineExecuted = new();
        public Observable<ScenarioLine> OnLineExecuted => _onLineExecuted;
        private readonly Subject<(int choiceIndex, string label)> _onChoiceSelected = new();
        public Observable<(int choiceIndex, string label)> OnChoiceSelected => _onChoiceSelected;
        private readonly Subject<ScenarioAssetRegistry> _onAssetRegistryUpdated = new();
        public Observable<ScenarioAssetRegistry> OnAssetRegistryUpdated => _onAssetRegistryUpdated;
        private readonly Subject<Unit> _onScenarioEnded = new();
        public Observable<Unit> OnScenarioEnded => _onScenarioEnded;
        protected AssetLoaderBase AssetLoader { get; private set; }
        public async UniTask LoadAllAsset() => await AssetLoader.LoadAllAssets();
        private readonly Dictionary<string, GameObject> _showCharacters = new();
        public IReadOnlyDictionary<string, GameObject> ShowCharacters => _showCharacters;
        private Func<string, ScenarioData> _resolveScenarioFunc;

        public void Initialize(AssetLoaderBase assetLoader = null)
        {
            AssetLoader = assetLoader ?? new UnityResourceLoader();
            RegisterCommands();
            OnInitialize();
        }

        public void SetScenarioDataProvider(Func<string, ScenarioData> func)
        {
            _resolveScenarioFunc = func;
        }

        public void SetCharacter(string targetCharacterName, GameObject characterInstance)
        {
            _showCharacters[targetCharacterName] = characterInstance;
        }

        protected abstract void OnInitialize();

        protected virtual void RegisterCommands()
        {
            RegisterCommand(new InitVariableCommand());
            RegisterCommand(new ShowTextCommand());
            RegisterCommand(new ChoiceCommand());
            RegisterCommand(new BranchCommand(BranchRegistry));
            RegisterCommand(new IfEqualCommand(BranchRegistry));
            RegisterCommand(new IfNotEqualCommand(BranchRegistry));
            RegisterCommand(new IfGreaterCommand(BranchRegistry));
            RegisterCommand(new IfGreaterEqualCommand(BranchRegistry));
            RegisterCommand(new IfLessCommand(BranchRegistry));
            RegisterCommand(new IfLessEqualCommand(BranchRegistry));
            RegisterCommand(new SetCommand());
            RegisterCommand(new AddCommand());
            RegisterCommand(new PrintCommand());
            RegisterCommand(new RequestAssetCommand());
            RegisterCommand(new RequestSpriteCommand());
            RegisterCommand(new LoadAllAssetCommand());
            RegisterCommand(new ShowCharacterCommand());
            RegisterCommand(new HideCharacterCommand());
            RegisterCommand(new MoveCharacterCommand());
            RegisterCommand(new ShowBackgroundCommand());
            RegisterCommand(new PlayVoiceCommand());
            RegisterCommand(new PlayBGMCommand());
            RegisterCommand(new PlaySECommand());
            RegisterCommand(new ShowMenuCommand());
            RegisterCommand(new WaitCommand());
            RegisterCommand(new FinishCommand());
            RegisterCommand(new FadeInCommand());
            RegisterCommand(new FadeOutCommand());
            RegisterCommand(new TaskCommand());
            RegisterCommand(new ExecCommand());
            RegisterCommand(new CallScenarioCommand());
            RegisterCommand(new ShowIconCommand());
            RegisterCommand(new ShowStillCommand());
            RegisterCommand(new StopBGMCommand());
            RegisterCommand(new StopSECommand());
            RegisterCommand(new RequestSoundCommand());
            RegisterCommand(new GrayCommand());
            RegisterCommand(new SepiaCommand());
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
            var ids = line?.NextIDs;
            if (ids is not { Count: > 0 })
            {
                return (line?.ID ?? _currentId) + 1;
            }

            var index = branchIndex ?? 0;
            if (index >= line.NextIDs.Count)
            {
                return line.ID + 1;
            }

            var relativeOffset = line.NextIDs[index];
            return relativeOffset > 0 ? line.ID + relativeOffset : line.ID + 1;
        }

        public void SetVariable(string key, object value)
        {
            _variables[key] = value;
        }

        public List<string> ResolveVariables(List<string> args)
        {
            if (args == null || args.Count == 0)
            {
                return new List<string>();
            }

            var result = new List<string>(args.Count);
            for (var i = 0; i < args.Count; i++)
            {
                result.Add(_variables.TryGetValue(args[i], out var variable) ? variable.ToString() : args[i]);
            }

            return result;
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

        protected void RegisterCommand(IScenarioCommandAsync command)
        {
            command.SetManager(this);
            _commands.Register(command);
        }

        protected void RegisterBranch(IScenarioBranchEvaluator evaluator)
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
            _scenarioContexts.Clear();
            SetScenarioLines(lines, startId);
        }

        private void SetScenarioLines(List<ScenarioLine> lines, int startId)
        {
            _lines = lines?.ToDictionary(x => x.ID) ?? new Dictionary<int, ScenarioLine>();
            _currentId = startId;
            _isScenarioEnded = false;
            _hasDispatchedScenarioEnd = false;
            ProceedAsync().Forget();
        }

        public virtual void Select(int choiceIndex)
        {
            if (!_lines.TryGetValue(_currentId, out var current) || current.CommandName != "Choice")
            {
                return;
            }

            var choiceCount = current.Args?.Count ?? 0;
            if (choiceCount == 0 || choiceIndex < 0 || choiceIndex >= choiceCount)
            {
                return;
            }

            _onChoiceSelected.OnNext((choiceIndex, current.Args[choiceIndex]));
            _currentId = GetNextId(current, choiceIndex);
            IsWaitingForInput = false;
        }

        private async UniTask ProceedAsync()
        {
            while (!_isScenarioEnded && _lines.TryGetValue(_currentId, out var line))
            {
                _onLineExecuted.OnNext(line);
                await _commands.ExecuteAsync(line.CommandName, line.Args);
                await UniTask.Yield();
            }

            if (_isScenarioEnded)
            {
                return;
            }

            _isScenarioEnded = true;
            if (!ResumePreviousScenario())
            {
                OnScenarioFinished();
            }
        }

        public void Stop()
        {
            _isScenarioEnded = true;
            if (!ResumePreviousScenario())
            {
                OnScenarioFinished();
            }
        }

        public void SetCurrentId(int id)
        {
            _currentId = id;
        }

        public int GetNextLineId(int? branchIndex = null)
        {
            return GetNextId(GetCurrentLine(), branchIndex);
        }

        public void SetNextLineId()
        {
            _currentId = GetNextLineId();
        }

        internal ScenarioData ResolveScenarioData(string scenarioId)
        {
            return _resolveScenarioFunc(scenarioId);
        }

        internal void CallScenario(ScenarioData scenarioData, int startId = 1)
        {
            if (scenarioData == null)
            {
                _currentId = GetNextLineId();
                return;
            }

            var resumeId = GetNextLineId();
            _scenarioContexts.Push(new ScenarioRuntimeContext
            {
                Lines = _lines,
                ResumeId = resumeId
            });

            SetScenarioLines(scenarioData.Lines, startId);
        }

        private bool ResumePreviousScenario()
        {
            if (_scenarioContexts.Count == 0)
            {
                return false;
            }

            var context = _scenarioContexts.Pop();
            _lines = context.Lines;
            _currentId = context.ResumeId;
            _isScenarioEnded = false;
            ProceedAsync().Forget();
            return true;
        }

        protected void OnScenarioFinished()
        {
            if (_hasDispatchedScenarioEnd)
            {
                return;
            }

            _hasDispatchedScenarioEnd = true;
            _onScenarioEnded.OnNext(Unit.Default);
        }
    }
}