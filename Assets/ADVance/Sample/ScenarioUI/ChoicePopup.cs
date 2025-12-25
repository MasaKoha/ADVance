using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using R3;
using UnityEngine.UI;

namespace ADVance.Sample.Scenario
{
    // 選択肢ボタン群の生成と選択通知を行う実体コンポーネント。
    public class ChoicePopup : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _content = null;

        [SerializeField]
        private List<Button> _choiceButtons = null;

        [SerializeField]
        private List<TextMeshProUGUI> _choiceTexts = null;

        private readonly Subject<int> _onSelectedChoice = new();
        public Observable<int> OnSelectedChoice => _onSelectedChoice;

        public void Initialize()
        {
            for (var i = 0; i < _choiceButtons.Count; i++)
            {
                var index = i; // Capture the current value of i
                _choiceButtons[i].OnClickAsObservable()
                    .Subscribe(_ => _onSelectedChoice.OnNext(index))
                    .AddTo(this);
            }
        }

        public UniTask ShowAsync(List<string> choices)
        {
            for (var i = 0; i < _choiceButtons.Count; i++)
            {
                if (i < choices.Count)
                {
                    _choiceButtons[i].gameObject.SetActive(true);
                    _choiceTexts[i].text = choices[i];
                }
                else
                {
                    _choiceButtons[i].gameObject.SetActive(false);
                }
            }

            _content.gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public UniTask HideAsync()
        {
            _content.gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }
}
