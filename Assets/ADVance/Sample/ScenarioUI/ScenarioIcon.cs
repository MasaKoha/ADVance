using UnityEngine;
using UnityEngine.UI;

namespace ADVance.Sample.Scenario
{
    public class ScenarioIcon : MonoBehaviour
    {
        [SerializeField] private Image _iconImage = null;
        [SerializeField] private RectTransform _rectTransform = null;

        public float X { get; private set; }
        public float Y { get; private set; }

        public void Initialize(Sprite sprite)
        {
            _iconImage.sprite = sprite;
        }

        public void Show(float x, float y)
        {
            X = x;
            Y = y;
            gameObject.SetActive(true);
            _rectTransform.anchoredPosition = new Vector2(x, y);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetColor(Color color)
        {
            _iconImage.color = color;
        }
    }
}