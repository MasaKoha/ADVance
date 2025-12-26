using System;
using System;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ADVance.Sample.Scenario
{
    // 台詞テキストと話者名の描画、タイプライター演出を担当するプレゼンター。
    public sealed class ScenarioUITextPresenter : MonoBehaviour, IDisposable
    {
        private const float CharactersPerSecond = 20f;
        private const bool EnableTypewriterEffect = true;
        [SerializeField] private TextMeshProUGUI _text = null;
        [SerializeField] private TextMeshProUGUI _speakerNameText = null;

        private ScenarioSpeedController _speedController = null;
        private string _currentDisplayText = string.Empty;
        private bool _isTypewriterActive;
        private int _typewriterOperationId;
        private readonly StringBuilder _typewriterBuilder = new(128);
        private Func<IDisposable> _autoAdvanceBlockFactory;

        public void Initialize(Func<IDisposable> autoAdvanceBlockFactory, ScenarioSpeedController speedController)
        {
            _autoAdvanceBlockFactory = autoAdvanceBlockFactory;
            _speedController = speedController;
        }

        public void ShowText(string speaker, string text)
        {
            if (_speakerNameText != null)
            {
                _speakerNameText.text = speaker;
            }

            _currentDisplayText = text ?? string.Empty;
            CancelTypewriterEffect(false);

            if (EnableTypewriterEffect && _text != null)
            {
                BeginTypewriterEffect(_currentDisplayText);
            }
            else
            {
                UpdateDisplayedText(_currentDisplayText);
            }
        }

        // 進行中のタイプライター演出を強制的に完了させる。
        public bool TryCompleteTypewriter()
        {
            if (!_isTypewriterActive)
            {
                return false;
            }

            CancelTypewriterEffect(true);
            return true;
        }

        // タイプライター演出を開始し、キャンセル用トークンと自動進行ブロッカーを確保する。
        private void BeginTypewriterEffect(string text)
        {
            if (_text == null)
            {
                return;
            }

            _isTypewriterActive = true;
            var operationId = ++_typewriterOperationId;
            var autoAdvanceBlock = _autoAdvanceBlockFactory?.Invoke();
            TypewriterEffectAsync(text, operationId, autoAdvanceBlock).Forget();
        }

        // タイプライター演出を停止し、必要に応じて全文表示へ切り替える。
        private void CancelTypewriterEffect(bool completeDisplay)
        {
            if (!_isTypewriterActive)
            {
                return;
            }

            _typewriterOperationId++;
            _isTypewriterActive = false;

            if (completeDisplay)
            {
                CompleteTypewriterEffect();
            }
        }

        // 文字列を 1 文字ずつ描画するタイプライター本体の非同期処理。
        private async UniTask TypewriterEffectAsync(string text, int operationId, IDisposable autoAdvanceBlock)
        {
            _typewriterBuilder.Clear();
            _text.text = string.Empty;
            var baseSecondsPerCharacter = 1f / Mathf.Max(CharactersPerSecond, 0.01f);
            var secondsPerCharacter = _speedController.AdjustSeconds(baseSecondsPerCharacter);

            try
            {
                for (var i = 0; i < text.Length; i++)
                {
                    if (_typewriterOperationId != operationId)
                    {
                        return;
                    }

                    _typewriterBuilder.Append(text[i]);
                    _text.SetText(_typewriterBuilder);
                    await DelayCharacterInterval(secondsPerCharacter, operationId);
                }
            }
            finally
            {
                if (_typewriterOperationId == operationId)
                {
                    OnTypewriterFinished(autoAdvanceBlock);
                }
                else
                {
                    autoAdvanceBlock?.Dispose();
                }
            }
        }

        // タイプライター完了時にトークンとブロッカーを後始末する。
        private void OnTypewriterFinished(IDisposable autoAdvanceBlock)
        {
            _isTypewriterActive = false;
            autoAdvanceBlock?.Dispose();
        }

        // タイプライターを強制完了した際に全文を描画する。
        private void CompleteTypewriterEffect()
        {
            _isTypewriterActive = false;
            UpdateDisplayedText(_currentDisplayText);
        }

        private void UpdateDisplayedText(string text)
        {
            if (_text == null)
            {
                return;
            }

            _text.text = text;
        }

        public void Dispose()
        {
            _isTypewriterActive = false;
        }

        private void OnDestroy()
        {
            Dispose();
        }

        // 1 文字ごとの待機を ID で監視しながら行う。
        private async UniTask DelayCharacterInterval(float secondsPerCharacter, int operationId)
        {
            var elapsed = 0f;
            var destroyToken = this.GetCancellationTokenOnDestroy();
            while (elapsed < secondsPerCharacter)
            {
                if (_typewriterOperationId != operationId)
                {
                    return;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, destroyToken);
                elapsed += Time.deltaTime;
            }
        }
    }
}
