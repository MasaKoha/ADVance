using System.Collections.Generic;
using ADVance.AssetLoader;
using ADVance.Command;
using ADVance.Data;
using ADVance.Manager;
using UnityEngine;
using R3;

namespace ADVance.Sample.Scenario
{
    // ScenarioManager と各 UI を繋ぎ、コマンド実行とユーザー操作の同期を取る。
    public class ScenarioController : MonoBehaviour
    {
        [SerializeField] private ScenarioManager _scenarioManager = null;

        [SerializeField] private ScenarioUIController _scenarioUIController = null;

        // CallScenario で使う Id -> ScenarioData の辞書
        private readonly Dictionary<string, ScenarioData> _subScenarios = new();

        // ScenarioEntries の再帰登録時に重複・循環を防ぐためのセット
        private readonly HashSet<ScenarioData> _registeredScenarioData = new();
        private readonly ScenarioSpeedController _speedController = new();

        public void Initialize()
        {
            BuildScenarioLookup();
            _scenarioManager.Initialize(new UnityResourceLoader());
            _scenarioManager.SetScenarioDataProvider(ResolveScenario);
            _scenarioUIController.Initialize(_speedController);
            SetEvent();
            SetCommandEvent();
        }

        private void BuildScenarioLookup()
        {
            _subScenarios.Clear();
            _registeredScenarioData.Clear();
        }

        private void RegisterScenarioEntries(IEnumerable<ScenarioEntry> entries)
        {
            if (entries == null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                RegisterScenarioEntry(entry);
            }
        }

        private void RegisterScenarioEntry(ScenarioEntry entry)
        {
            if (entry.Data == null || string.IsNullOrEmpty(entry.Id))
            {
                return;
            }

            _subScenarios[entry.Id] = entry.Data;

            if (!_registeredScenarioData.Add(entry.Data))
            {
                return;
            }

            RegisterScenarioEntries(entry.Data.ScenarioEntries);
        }

        private void SetEvent()
        {
            _scenarioUIController.OnContinueScenario
                .Subscribe(_ => { _scenarioManager.ContinueScenario(); })
                .AddTo(this);

            _scenarioUIController.OnSelectedChoice
                .Subscribe(index => { _scenarioManager.Select(index); })
                .AddTo(this);

            _scenarioManager.OnScenarioEnded
                .Subscribe(_ => { Debug.Log("Complete"); })
                .AddTo(this);

            _scenarioManager.OnChoiceSelected
                .Subscribe(value => { _scenarioUIController.HideChoices(); })
                .AddTo(this);

            _scenarioManager.OnShowChoice
                .Subscribe(value => { _scenarioUIController.ShowChoices(value); })
                .AddTo(this);
        }

        private void SetCommandEvent()
        {
            // ShowTextCommandの購読
            var showTextCommand = _scenarioManager.CommandRegistry.GetCommand<ShowTextCommand>();
            showTextCommand.OnTextShow
                .Subscribe(ShowText)
                .AddTo(this);

            var showBackgroundCommand = _scenarioManager.CommandRegistry.GetCommand<ShowBackgroundCommand>();
            showBackgroundCommand.OnShowBackground
                .Subscribe(key =>
                {
                    var sprite = _scenarioManager.GetSpriteAsset(key);
                    _scenarioUIController.ShowBackground(sprite);
                })
                .AddTo(this);

            var showCharacterCommand = _scenarioManager.CommandRegistry.GetCommand<ShowCharacterCommand>();
            showCharacterCommand.OnShowCharacter
                .Subscribe(param =>
                {
                    var sprite = _scenarioManager.GetSpriteAsset(param.Key);
                    _scenarioUIController.ShowCharacter(param.Key, sprite, param.X, param.Y);
                })
                .AddTo(this);

            var hideCharacterCommand = _scenarioManager.CommandRegistry.GetCommand<HideCharacterCommand>();
            hideCharacterCommand.OnHideCharacter
                .Subscribe(key => { _scenarioUIController.HideCharacter(key); })
                .AddTo(this);

            var waitCommand = _scenarioManager.CommandRegistry.GetCommand<WaitCommand>();
            waitCommand.OnWaitStart
                .Subscribe(_ => { _scenarioUIController.ShowNextButton(); })
                .AddTo(this);

            var moveCharacterCommand = _scenarioManager.CommandRegistry.GetCommand<MoveCharacterCommand>();
            moveCharacterCommand.OnCharacterMoved
                .Subscribe(async param => { await _scenarioUIController.MoveCharacter(param); })
                .AddTo(this);

            var fadeInCommand = _scenarioManager.CommandRegistry.GetCommand<FadeInCommand>();
            fadeInCommand.OnFadeIn
                .Subscribe(param => { _scenarioUIController.FadeIn(param); })
                .AddTo(this);

            var fadeOutCommand = _scenarioManager.CommandRegistry.GetCommand<FadeOutCommand>();
            fadeOutCommand.OnFadeOut
                .Subscribe(param => { _scenarioUIController.FadeOut(param); })
                .AddTo(this);

            var showIconCommand = _scenarioManager.CommandRegistry.GetCommand<ShowIconCommand>();
            showIconCommand.OnShowIcon
                .Subscribe(param =>
                {
                    var sprite = _scenarioManager.GetSpriteAsset(param.Key);
                    _scenarioUIController.ShowIcon(param.Key, sprite, param.X, param.Y);
                })
                .AddTo(this);

            var showStillCommand = _scenarioManager.CommandRegistry.GetCommand<ShowStillCommand>();
            showStillCommand.OnShowStill
                .Subscribe(param =>
                {
                    var sprite = _scenarioManager.GetSpriteAsset(param.Key);
                    _scenarioUIController.ShowStill(param.Key, sprite);
                })
                .AddTo(this);

            var sepiaCommand = _scenarioManager.CommandRegistry.GetCommand<SepiaCommand>();
            sepiaCommand.OnSepia
                .Subscribe(param => _scenarioUIController.ChangeColorSepia(param.Enabled))
                .AddTo(this);

            var grayCommand = _scenarioManager.CommandRegistry.GetCommand<GrayCommand>();
            grayCommand.OnGray
                .Subscribe(param => _scenarioUIController.ChangeColorGray(param.Enabled))
                .AddTo(this);
        }

        private void ShowText(ShowTextParameter textParameter)
        {
            // 変数置換処理
            var processedText = ProcessVariables(textParameter.Text);
            _scenarioUIController.ShowText(textParameter.Speaker, processedText);
        }

        // テキスト中の ${varName} 形式を ScenarioManager の変数値に差し替える。
        private string ProcessVariables(string text)
        {
            var result = text;
            foreach (var kvp in _scenarioManager.Variables)
            {
                var placeholder = "${" + kvp.Key + "}";
                if (result.Contains(placeholder))
                {
                    result = result.Replace(placeholder, kvp.Value.ToString());
                }
            }

            return result;
        }

        public void StartScenario(ScenarioData scenarioData)
        {
            // TODO:シナリオ開始前にplayer変数を設定。本来はユーザ名をここに入れる
            _scenarioManager.SetVariable("player", "masakoha");
            RegisterScenario(scenarioData.name, scenarioData);
            _scenarioManager.StartScenario(scenarioData);
        }

        private void RegisterScenario(string id, ScenarioData data)
        {
            if (string.IsNullOrEmpty(id) || data == null)
            {
                return;
            }

            _subScenarios[id] = data;

            if (_registeredScenarioData.Add(data))
            {
                RegisterScenarioEntries(data.ScenarioEntries);
            }
        }

        private ScenarioData ResolveScenario(string scenarioId)
        {
            return _subScenarios.GetValueOrDefault(scenarioId);
        }
    }
}