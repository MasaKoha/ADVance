using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ADVance
{
    public class ScenarioUIController : MonoBehaviour
    {
        [Header("Dialog UI")]
        [SerializeField] private TextMeshProUGUI _dialogText;

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

        private Coroutine _typewriterCoroutine;
        private string _currentDisplayText;
        private readonly Subject<Unit> _onContinueScenario = new();
        public Observable<Unit> OnContinueScenario => _onContinueScenario;
        private readonly Subject<int> _onSelectedChoice = new();
        public Observable<int> OnSelectedChoice => _onSelectedChoice;
        private readonly Subject<Unit> _onStartDownload = new();
        public Observable<Unit> OnStartDownload => _onStartDownload;

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
            if (_nextButton != null)
            {
                _nextButton.onClick.AddListener(() =>
                {
                    if (_typewriterCoroutine != null)
                    {
                        StopCoroutine(_typewriterCoroutine);
                        _typewriterCoroutine = null;
                        CompleteTypewriterEffect();
                        return;
                    }

                    _nextButton.gameObject.SetActive(false);
                    _onContinueScenario.OnNext(Unit.Default);
                });
            }
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
                _choiceButtons[i].onClick.AddListener(() => { _onSelectedChoice.OnNext(index); });
            }
        }

        private void SetupPreloadUI()
        {
            if (_preloadPanel != null)
            {
                _preloadPanel.SetActive(false);
            }

            if (_downloadProgressSlider != null)
            {
                _downloadProgressSlider.value = 0f;
            }

            if (_startDownloadButton != null)
            {
                _startDownloadButton.onClick.AddListener(() => _onStartDownload.OnNext(Unit.Default));
            }
        }

        public void ShowText(string text)
        {
            _currentDisplayText = text;
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
            }

            if (_enableTypewriterEffect && _dialogText != null)
            {
                _typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
            }
            else
            {
                if (_dialogText != null)
                {
                    _dialogText.text = text;
                }

                ShowNextButton();
            }
        }

        private IEnumerator TypewriterEffect(string text)
        {
            _dialogText.text = "";

            var delay = 1f / _charactersPerSecond;

            for (var i = 0; i <= text.Length; i++)
            {
                _dialogText.text = text.Substring(0, i);
                yield return new WaitForSeconds(delay);
            }

            _typewriterCoroutine = null;
            ShowNextButton();
        }

        private void CompleteTypewriterEffect()
        {
            if (_currentDisplayText != null)
            {
                _dialogText.text = _currentDisplayText;
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
            _preloadPanel.SetActive(true);
            _preloadSizeText.text = sizeText;
        }

        public void HidePreloadPanel()
        {
            _preloadPanel.SetActive(false);
        }

        public void UpdateDownloadProgress(float progress)
        {
            _downloadProgressSlider.value = progress;
            _downloadProgressText.text = $"{(progress * 100):F0}%";
        }

        public void SetStartDownloadButtonInteractable(bool interactable)
        {
            _startDownloadButton.interactable = interactable;
        }
    }
}