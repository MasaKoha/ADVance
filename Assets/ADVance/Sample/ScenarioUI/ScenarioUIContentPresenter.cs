using System;
using System.Collections.Generic;
using ADVance.Command;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ADVance.Sample.Scenario
{
    // 背景・キャラクター・フェードなど視覚要素をまとめて管理するプレゼンター。
    public sealed class ScenarioUIContentPresenter : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage = null;
        [SerializeField] private Transform _contentRootCharacter = null;
        [SerializeField] private Transform _contentRootIcon = null;
        [SerializeField] private Transform _contentRootStill = null;
        [SerializeField] private Image _fadeImage = null;
        [SerializeField] private Character _characterPrefab = null;
        [SerializeField] private ScenarioIcon _scenarioIconPrefab = null;
        [SerializeField] private ScenarioStill _scenarioStillPrefab = null;
        private readonly Dictionary<string, Character> _showCharacters = new();
        private readonly Dictionary<string, ScenarioIcon> _showIcons = new();
        private readonly Dictionary<string, ScenarioStill> _showStills = new();
        private readonly Color _colorSepia = new Color() { r = 107f / 255f, g = 74f / 255f, b = 43f / 255f, a = 1f };
        private ScenarioSpeedController _speedController = null;
        private Func<IDisposable> _autoAdvanceBlockFactory;
        private Color _changeColor = Color.white;
        private bool _isChangeColorSepia = false;
        private bool _isChangeColorGray = false;

        public void Initialize(Func<IDisposable> autoAdvanceBlockFactory, ScenarioSpeedController speedController)
        {
            _autoAdvanceBlockFactory = autoAdvanceBlockFactory;
            _speedController = speedController;
            _fadeImage.gameObject.SetActive(true);
        }

        public void ShowBackground(Sprite sprite)
        {
            if (_backgroundImage == null)
            {
                return;
            }

            _backgroundImage.sprite = sprite;
        }

        public void ShowCharacter(string key, Sprite sprite, float x, float y)
        {
            if (_characterPrefab == null || _contentRootCharacter == null)
            {
                return;
            }

            var character = Instantiate(_characterPrefab, _contentRootCharacter);
            character.Initialize(sprite, _speedController);
            character.SetColor(_changeColor);
            character.ShowCharacter(x, y);
            _showCharacters[key] = character;
        }

        public void HideCharacter(HideCharacterParameter parameter)
        {
            if (!_showCharacters.TryGetValue(parameter.Key, out var character))
            {
                return;
            }

            character.HideCharacter();
            _showCharacters.Remove(parameter.Key);
            Destroy(character.gameObject);
        }

        public async UniTask MoveCharacter(MoveCharacterParameter parameter)
        {
            if (!_showCharacters.TryGetValue(parameter.Key, out var character))
            {
                return;
            }

            var autoAdvanceBlocker = parameter.Duration > 0f ? _autoAdvanceBlockFactory?.Invoke() : null;
            try
            {
                await character.MoveCharacter(parameter.TargetX, parameter.TargetY, parameter.Duration);
            }
            finally
            {
                autoAdvanceBlocker?.Dispose();
            }
        }

        public void SkipAllCharacterMoves()
        {
            foreach (var character in _showCharacters.Values)
            {
                character.SkipCurrentMove();
            }
        }

        public void FadeIn(float duration, Color color, Action onComplete)
        {
            if (_fadeImage == null)
            {
                onComplete?.Invoke();
                return;
            }

            StartFade(duration, color, 0f, 1f, onComplete);
        }

        public void FadeOut(float duration, Color color, Action onComplete)
        {
            if (_fadeImage == null)
            {
                onComplete?.Invoke();
                return;
            }

            StartFade(duration, color, 1f, 0f, onComplete);
        }

        private void StartFade(float duration, Color color, float startAlpha, float targetAlpha, Action onComplete)
        {
            _fadeImage.gameObject.SetActive(true);
            _fadeImage.color = new Color(color.r, color.g, color.b, startAlpha);
            var adjustedDuration = _speedController != null ? _speedController.AdjustSeconds(duration) : duration;

            if (adjustedDuration <= 0f)
            {
                _fadeImage.color = new Color(color.r, color.g, color.b, targetAlpha);
                onComplete?.Invoke();
                return;
            }

            FadeImageAsync(adjustedDuration, color, startAlpha, targetAlpha, onComplete).Forget();
        }

        private async UniTask FadeImageAsync(float duration, Color color, float startAlpha, float targetAlpha, Action onComplete)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                _fadeImage.color = new Color(color.r, color.g, color.b, alpha);
                await UniTask.Yield();
            }

            _fadeImage.color = new Color(color.r, color.g, color.b, targetAlpha);
            onComplete?.Invoke();
        }

        public void ShowIcon(string key, Sprite sprite, float x, float y)
        {
            if (_scenarioIconPrefab == null || _contentRootIcon == null)
            {
                return;
            }

            var scenarioIcon = Instantiate(_scenarioIconPrefab, _contentRootIcon);
            scenarioIcon.Initialize(sprite);
            scenarioIcon.SetColor(_changeColor);
            scenarioIcon.Show(x, y);
            _showIcons[key] = scenarioIcon;
        }

        public void ShowStill(string key, Sprite sprite)
        {
            if (_scenarioStillPrefab == null || _contentRootStill == null)
            {
                return;
            }

            var scenarioStill = Instantiate(_scenarioStillPrefab, _contentRootStill);
            scenarioStill.SetColor(_changeColor);
            scenarioStill.Initialize(sprite);
            scenarioStill.Show();
            _showStills[key] = scenarioStill;
        }

        private void ChangeColor(Color color)
        {
            if (_changeColor == color)
            {
                return;
            }

            _changeColor = color;

            if (_backgroundImage != null)
            {
                _backgroundImage.color = _changeColor;
            }

            if (_showCharacters.Count > 0)
            {
                foreach (var character in _showCharacters.Values)
                {
                    character.SetColor(_changeColor);
                }
            }

            if (_showIcons.Count > 0)
            {
                foreach (var icon in _showIcons.Values)
                {
                    icon.SetColor(_changeColor);
                }
            }

            if (_showStills.Count > 0)
            {
                foreach (var still in _showStills.Values)
                {
                    still.SetColor(_changeColor);
                }
            }
        }

        public void ChangeColorSepia()
        {
            ChangeColor(_colorSepia);
            _isChangeColorSepia = true;
            _isChangeColorGray = false;
        }

        public void ChangeColorGray()
        {
            ChangeColor(Color.gray);
            _isChangeColorGray = true;
            _isChangeColorSepia = false;
        }

        public void ChangeColorSepiaToWhite()
        {
            if (!_isChangeColorSepia)
            {
                return;
            }

            ChangeColor(Color.white);
            _isChangeColorSepia = false;
        }

        public void ChangeColorGrayToWhite()
        {
            if (!_isChangeColorGray)
            {
                return;
            }

            ChangeColor(Color.white);
            _isChangeColorGray = false;
        }

        private void OnDestroy()
        {
            foreach (var character in _showCharacters.Values)
            {
                if (character != null)
                {
                    Destroy(character.gameObject);
                }
            }

            _showCharacters.Clear();
        }
    }
}
