using UnityEngine;
using UnityEngine.UI;

namespace ADVance.Sample.Scenario
{
    public class ScenarioStill : MonoBehaviour
    {
        [SerializeField] private Image _stillImage = null;

        public void Initialize(Sprite sprite)
        {
            _stillImage.sprite = sprite;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetColor(Color color)
        {
            _stillImage.color = color;
        }
    }
}