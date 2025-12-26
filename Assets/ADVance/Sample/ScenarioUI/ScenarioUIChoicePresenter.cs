using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Sample.Scenario
{
    // 選択肢ポップアップの表示・非表示と選択通知を担当するプレゼンター。
    public sealed class ScenarioUIChoicePresenter : MonoBehaviour
    {
        [SerializeField] private ChoicePopup _choicePopup = null;
        public Observable<int> OnChoiceSelected => _choicePopup != null ? _choicePopup.OnSelectedChoice : Observable.Empty<int>();

        public void Initialize()
        {
            if (_choicePopup == null)
            {
                return;
            }

            _choicePopup.Initialize();
            HideChoices();
        }

        public void ShowChoices(List<string> choices)
        {
            _choicePopup?.ShowAsync(choices).Forget();
        }

        public void HideChoices()
        {
            _choicePopup?.HideAsync().Forget();
        }
    }
}