using System;
using System.Collections.Generic;
using ADVance.Command;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace ADVance.Sample.Scenario
{
    // 各種 UI プレゼンターを束ね、ADV 進行中の表示と操作を統括する。
    public class ScenarioUIController : MonoBehaviour
    {
        [SerializeField] private ScenarioUIHeader _header = null;
        [SerializeField] private ScenarioUITextPresenter _text = null;
        [SerializeField] private Button _nextButton = null;
        [SerializeField] private ScenarioUIChoicePresenter _choices = null;
        [SerializeField] private ScenarioUIContentPresenter _contents = null;
        [SerializeField] private ScenarioUILogPresenter _log = null;
        [SerializeField] private Button _turboButton = null;
        private ScenarioSpeedController _speedController = null;
        private ScenarioAutoAdvController _autoAdvController;
        private bool _isTurboActive = false;
        private readonly Subject<Unit> _onContinueScenario = new();
        public Observable<Unit> OnContinueScenario => _onContinueScenario;
        private readonly Subject<int> _onSelectedChoice = new();
        public Observable<int> OnSelectedChoice => _onSelectedChoice;
        private readonly Subject<Unit> _onStartDownload = new();
        public Observable<Unit> OnStartDownload => _onStartDownload;
        private float _turboSpeedMultiplier;

        public void Initialize(ScenarioSpeedController speedController, float turboSpeedMultiplier = 50f)
        {
            _turboSpeedMultiplier = turboSpeedMultiplier;
            _header.Initialize();
            _speedController = speedController;
            _autoAdvController = new ScenarioAutoAdvController();
            _autoAdvController.Initialize(_header, HandleContinueAction, _speedController);

            Func<IDisposable> autoAdvanceBlockFactory = () => _autoAdvController.BeginAutoAdvanceBlock();
            _text.Initialize(autoAdvanceBlockFactory, _speedController);
            _choices.Initialize();
            _contents.Initialize(autoAdvanceBlockFactory, _speedController);
            InitializeNextButton();
            _autoAdvController.SetAwaitingAdvance(false);
            _log.Initialize();
            SetScenarioSpeed(1f);
            SetEvent();
        }

        public void SetScenarioSpeed(float multiplier)
        {
            _speedController.SetSpeedMultiplier(multiplier);
        }

        public void StartTurboAuto()
        {
            if (_autoAdvController == null)
            {
                return;
            }

            var token = this.GetCancellationTokenOnDestroy();
            _autoAdvController.RunTurboAutoAsync(token).Forget();
        }

        private void ToggleTurboMode()
        {
            _isTurboActive = !_isTurboActive;
            var speed = _isTurboActive ? _turboSpeedMultiplier : 1f;
            SetScenarioSpeed(speed);

            if (_isTurboActive)
            {
                StartTurboAuto();
            }
        }

        private void SetEvent()
        {
            _nextButton.OnClickAsObservable()
                .Subscribe(_ => HandleContinueAction())
                .AddTo(this);

            _choices.OnChoiceSelected
                .Subscribe(OnChoiceSelected)
                .AddTo(this);

            _header.OnShowLogChanged
                .Subscribe(OnShowLog)
                .AddTo(this);

            _log.OnClickClosedButton
                .Subscribe(_ => OnShowLog(false))
                .AddTo(this);

            if (_turboButton != null)
            {
                _turboButton.OnClickAsObservable()
                    .Subscribe(_ => ToggleTurboMode())
                    .AddTo(this);
            }
        }

        private void InitializeNextButton()
        {
            _nextButton.interactable = false;
            _nextButton.gameObject.SetActive(true);
        }

        public void ShowText(string speaker, string text)
        {
            _autoAdvController.SetAwaitingAdvance(true);
            _text.ShowText(speaker, text);
            _log.AddLog(speaker, text);
            ShowNextButton();
        }

        public void ShowChoices(List<string> choices)
        {
            _choices.ShowChoices(choices);
        }

        public void HideChoices()
        {
            _choices.HideChoices();
        }

        public void ShowNextButton()
        {
            _nextButton.gameObject.SetActive(true);
            _nextButton.interactable = true;
            _autoAdvController.SetAwaitingAdvance(true);
        }

        public void ShowBackground(Sprite sprite)
        {
            _contents.ShowBackground(sprite);
        }

        public void ShowCharacter(string key, Sprite sprite, float x, float y)
        {
            _contents.ShowCharacter(key, sprite, x, y);
        }

        public void HideCharacter(HideCharacterParameter parameter)
        {
            _contents.HideCharacter(parameter);
        }

        public async UniTask MoveCharacter(MoveCharacterParameter parameter)
        {
            ShowNextButton();
            await _contents.MoveCharacter(parameter);
        }

        public void SkipAllCharacterMoves()
        {
            _contents.SkipAllCharacterMoves();
        }

        public void FadeIn((FadeInParameter parameter, UniTaskCompletionSource completionSource) request)
        {
            // カラーコードからカラーを取得
            Color color;
            if (string.IsNullOrEmpty(request.parameter.ColorCode) ||
                !ColorUtility.TryParseHtmlString($"#{request.parameter.ColorCode}", out color))
            {
                color = Color.black;
            }

            SetNextButtonInteractable(false);
            _contents.FadeIn(request.parameter.Duration,
                color,
                () =>
                {
                    SetNextButtonInteractable(true);
                    request.completionSource.TrySetResult();
                });
        }

        public void FadeOut((FadeOutParameter parameter, UniTaskCompletionSource completionSource) request)
        {
            // カラーコードからカラーを取得
            Color color;
            if (string.IsNullOrEmpty(request.parameter.ColorCode) ||
                !ColorUtility.TryParseHtmlString($"#{request.parameter.ColorCode}", out color))
            {
                color = Color.black;
            }

            SetNextButtonInteractable(false);
            _contents.FadeOut(request.parameter.Duration,
                color,
                () =>
                {
                    SetNextButtonInteractable(true);
                    request.completionSource.TrySetResult();
                });
        }

        private void HandleContinueAction()
        {
            if (_text.TryCompleteTypewriter())
            {
                return;
            }

            WaitCommand.SkipCurrentWait();
            SkipAllCharacterMoves();
            _autoAdvController.SetAwaitingAdvance(false);
            _onContinueScenario.OnNext(Unit.Default);
        }

        private void OnChoiceSelected(int index)
        {
            _onSelectedChoice.OnNext(index);
            HideChoices();
        }

        private void OnShowLog(bool isShowLog)
        {
            _log.SetActiveShowLog(isShowLog);
        }

        private void SetNextButtonInteractable(bool interactable)
        {
            _nextButton.interactable = interactable;
        }

        public void ShowIcon(string key, Sprite sprite, float x, float y)
        {
            _contents.ShowIcon(key, sprite, x, y);
        }

        public void ShowStill(string key, Sprite sprite)
        {
            _contents.ShowStill(key, sprite);
        }

        public void ChangeColorSepia(bool isEnabled)
        {
            if (isEnabled)
            {
                _contents.ChangeColorSepia();
            }
            else
            {
                _contents.ChangeColorSepiaToWhite();
            }
        }

        public void ChangeColorGray(bool isEnabled)
        {
            if (isEnabled)
            {
                _contents.ChangeColorGray();
            }
            else
            {
                _contents.ChangeColorGrayToWhite();
            }
        }

        private void OnDestroy()
        {
            _text?.Dispose();
            _autoAdvController?.Dispose();
        }
    }
}