using R3;
using UnityEngine;
using UnityEngine.UI;

namespace ADVance.Sample.Scenario
{
    public class ScenarioUILogPresenter : MonoBehaviour
    {
        [SerializeField] private ScenarioUILogCell _logCellPrefab = null;
        [SerializeField] private RectTransform _contentRoot = null;
        [SerializeField] private Button _closeButton = null;
        public Observable<Unit> OnClickClosedButton => _closeButton.OnClickAsObservable();

        public void Initialize()
        {
            SetActiveShowLog(false);
        }

        public void SetActiveShowLog(bool isShowLog)
        {
            gameObject.SetActive(isShowLog);
        }

        public void AddLog(string speakerName, string message)
        {
            var logCell = Instantiate(_logCellPrefab, _contentRoot);
            logCell.Initialize(speakerName, message);
        }
    }
}