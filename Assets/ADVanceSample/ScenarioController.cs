using ADVance;
using ADVance.Command;
using ADVance.Data;
using ADVance.Manager;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

namespace ADVanceSample
{
    public class ScenarioController : MonoBehaviour
    {
        [SerializeField]
        private ScenarioManager _scenarioManager = null;

        [SerializeField]
        private ScenarioUIController _scenarioUIController = null;

        public void Initialize()
        {
            _scenarioManager.Initialize();
            _scenarioUIController.Initialize();
            SetEvent();
            SetCommandEvent();
        }

        private void SetEvent()
        {
            _scenarioUIController.OnContinueScenario
                .Subscribe(_ => { _scenarioManager.ContinueScenario(); })
                .AddTo(destroyCancellationToken);

            _scenarioUIController.OnSelectedChoice
                .Subscribe(index => { _scenarioManager.Select(index); })
                .AddTo(destroyCancellationToken);

            _scenarioManager.OnScenarioEnded
                .Subscribe(_ => { Debug.Log("Complete"); })
                .AddTo(destroyCancellationToken);

            _scenarioManager.OnStartDownloadButtonInteractable
                .Subscribe(value => { _scenarioUIController.SetStartDownloadButtonInteractable(value); })
                .AddTo(destroyCancellationToken);

            _scenarioManager.OnChoiceSelected
                .Subscribe(value => { _scenarioUIController.HideChoices(); })
                .AddTo(destroyCancellationToken);

            _scenarioManager.OnShowChoice
                .Subscribe(value => { _scenarioUIController.ShowChoices(value.choices, value.varName); })
                .AddTo(destroyCancellationToken);
        }

        private void SetCommandEvent()
        {
            // ShowTextCommandの購読
            var showTextCommand = _scenarioManager.CommandRegistry.GetCommand<ShowTextCommand>();
            showTextCommand.OnTextShow
                .Subscribe(ShowText)
                .AddTo(destroyCancellationToken);

            var showBackgroundCommand = _scenarioManager.CommandRegistry.GetCommand<ShowBackgroundCommand>();
            showBackgroundCommand.OnShowBackground
                .Subscribe(key =>
                {
                    var sprite = _scenarioManager.GetSpriteAsset(key);
                    _scenarioUIController.ShowBackground(sprite);
                })
                .AddTo(destroyCancellationToken);
        }

        private void ShowText(ShowTextParameter textParameter)
        {
            // 変数置換処理
            var processedText = ProcessVariables(textParameter.Text);
            _scenarioUIController.ShowText(textParameter.Speaker, processedText);
        }


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
            // シナリオ開始前にspeaker変数を設定
            _scenarioManager.SetVariable("speaker", "masakoha");
            _scenarioManager.StartScenario(scenarioData);
        }
    }
}