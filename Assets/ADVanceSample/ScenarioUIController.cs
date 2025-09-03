using System;
using System.Collections.Generic;
using System.Threading;
using R3;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ADVance
{
    public class ScenarioUIController : MonoBehaviour
    {
        [Header("Dialog UI")]
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private Button _nextButton;

        [Header("Choice UI")]
        [SerializeField] private Button[] _choiceButtons;

        [SerializeField] private GameObject _choicePanel;

        [Header("Asset Preload UI")]
        [SerializeField] private GameObject _preloadPanel;

        [SerializeField] private TextMeshProUGUI _preloadSizeText;
        [SerializeField] private Button _startDownloadButton;
        [SerializeField] private Slider _downloadProgressSlider;
        [SerializeField] private TextMeshProUGUI _downloadProgressText;

        [Header("Text Animation Settings")]
        [SerializeField] private float _charactersPerSecond = 20f;

        [SerializeField] private bool _enableTypewriterEffect = true;

        private CancellationTokenSource _typewriterCancellationTokenSource;
        private string _currentDisplayText;
        private bool _isTypewriterActive = false;
        private readonly Subject<Unit> _onContinueScenario = new();
        public Observable<Unit> OnContinueScenario => _onContinueScenario;
        private readonly Subject<int> _onSelectedChoice = new();
        public Observable<int> OnSelectedChoice => _onSelectedChoice;
        private readonly Subject<Unit> _onStartDownload = new();
        public Observable<Unit> OnStartDownload => _onStartDownload;

        private readonly CompositeDisposable _disposables = new();

        public void Initialize()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            SetupDialogUI();
            SetupChoiceUI();
            SetupPreloadUI();
        }

        private void SetupDialogUI()
        {
            _nextButton.onClick.AddListener(HandleContinueAction);
        }

        private void HandleContinueAction()
        {
            if (_isTypewriterActive && _typewriterCancellationTokenSource != null)
            {
                _typewriterCancellationTokenSource.Cancel();
                _typewriterCancellationTokenSource = null;
                _isTypewriterActive = false;
                CompleteTypewriterEffect();
                return;
            }

            _nextButton.gameObject.SetActive(false);
            _onContinueScenario.OnNext(Unit.Default);
        }

        private void SetupChoiceUI()
        {
            if (_choicePanel != null)
            {
                _choicePanel.SetActive(false);
            }

            for (var i = 0; i < _choiceButtons.Length; i++)
            {
                var index = i;
                _choiceButtons[i].onClick.AddListener(() => _onSelectedChoice.OnNext(index));
            }
        }

        private void SetupPreloadUI()
        {
            if (_preloadPanel == null)
            {
                return;
            }

            _preloadPanel.SetActive(false);
            _downloadProgressSlider.value = 0f;
            _startDownloadButton.onClick
                .AddListener(() => _onStartDownload.OnNext(Unit.Default));
        }

        public void ShowText(string text)
        {
            _currentDisplayText = text;
            if (_typewriterCancellationTokenSource != null)
            {
                _typewriterCancellationTokenSource.Cancel();
                _typewriterCancellationTokenSource = null;
            }

            _isTypewriterActive = false;
            ShowNextButton();

            if (_enableTypewriterEffect && _text != null)
            {
                _isTypewriterActive = true;
                _typewriterCancellationTokenSource = new CancellationTokenSource();
                TypewriterEffectAsync(text, _typewriterCancellationTokenSource.Token).Forget();
            }
            else
            {
                if (_text != null)
                {
                    _text.text = text;
                }
            }
        }

        private async UniTask TypewriterEffectAsync(string text, CancellationToken cancellationToken)
        {
            _text.text = "";

            var delay = 1000f / _charactersPerSecond; // Convert to milliseconds

            for (var i = 0; i <= text.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                _text.text = text.Substring(0, i);

                try
                {
                    await UniTask.Delay((int)delay, cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            _typewriterCancellationTokenSource = null;
            _isTypewriterActive = false;
        }

        private void CompleteTypewriterEffect()
        {
            if (_currentDisplayText != null)
            {
                _text.text = _currentDisplayText;
            }

            ShowNextButton();
        }

        private void ShowNextButton()
        {
            _nextButton.gameObject.SetActive(true);
        }

        public void ShowChoices(List<string> choices, string varName)
        {
            _choicePanel.SetActive(true);

            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                if (i < choices.Count)
                {
                    _choiceButtons[i].gameObject.SetActive(true);
                    var buttonText = _choiceButtons[i].GetComponentInChildren<Text>();
                    var buttonTMPText = _choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();

                    if (buttonTMPText != null)
                        buttonTMPText.text = choices[i];
                    else if (buttonText != null)
                        buttonText.text = choices[i];
                }
                else
                {
                    _choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void HideChoices()
        {
            _choicePanel.SetActive(false);
        }

        public void ShowPreloadConfirmation(string sizeText)
        {
            if (_preloadPanel != null)
            {
                _preloadPanel.SetActive(true);
            }

            _preloadSizeText.text = sizeText;
        }

        public void HidePreloadPanel()
        {
            if (_preloadPanel != null)
            {
                _preloadPanel.SetActive(false);
            }
        }

        public void UpdateDownloadProgress(float progress)
        {
            _downloadProgressSlider.value = progress;
            _downloadProgressText.text = $"{progress * 100:F0}%";
        }

        public void SetStartDownloadButtonInteractable(bool interactable)
        {
            _startDownloadButton.interactable = interactable;
        }

        private void OnDestroy()
        {
            _typewriterCancellationTokenSource?.Cancel();
            _typewriterCancellationTokenSource?.Dispose();
            _disposables?.Dispose();
        }
    }
}