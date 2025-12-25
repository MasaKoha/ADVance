using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace ADVance.Sample.Scenario
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private Image _characterImage = null;
        [SerializeField] private RectTransform _rectTransform = null;
        public float X { get; private set; }
        public float Y { get; private set; }

        private ScenarioSpeedController _speedController = null;

        public void Initialize(Sprite sprite, ScenarioSpeedController speedController)
        {
            _characterImage.sprite = sprite;
            _speedController = speedController;
        }

        public void ShowCharacter(float x, float y)
        {
            X = x;
            Y = y;
            gameObject.SetActive(true);
            _rectTransform.anchoredPosition = new Vector2(x, y);
        }

        public void HideCharacter()
        {
            gameObject.SetActive(false);
        }

        public UniTask MoveCharacter(float targetX, float targetY, float duration)
        {
            // 速度倍率を考慮した移動時間を計算
            var speed = _speedController.SpeedMultiplier;
            Debug.Log($"Speed: {speed}");
            return UniTask.CompletedTask;
        }

        public void SkipCurrentMove()
        {
        }

        public void SetColor(Color color)
        {
            _characterImage.color = color;
        }
    }
}