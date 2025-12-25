using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ADVance.Sample.Scenario
{
    public class ScenarioUILogCell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _speakerNameText = null;
        [SerializeField] private TextMeshProUGUI _messageText = null;
        [SerializeField] private LayoutElement _layoutElement = null;
        [SerializeField] private float _verticalPadding = 16f;

        public void Initialize(string speakerName, string message)
        {
            _speakerNameText.text = speakerName;
            _messageText.text = message;
            UpdatePreferredHeight();
        }

        private void UpdatePreferredHeight()
        {
            if (_layoutElement == null)
            {
                return;
            }

            var speakerHeight = GetPreferredHeight(_speakerNameText);
            var messageHeight = GetPreferredHeight(_messageText);
            _layoutElement.preferredHeight = speakerHeight + messageHeight + _verticalPadding;
        }

        private static float GetPreferredHeight(TextMeshProUGUI text)
        {
            if (text == null)
            {
                return 0f;
            }

            var rect = text.rectTransform.rect;
            var width = rect.width > 0f ? rect.width : float.PositiveInfinity;
            var preferred = text.GetPreferredValues(text.text, width, Mathf.Infinity);
            return preferred.y;
        }
    }
}
