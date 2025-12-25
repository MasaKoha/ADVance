using R3;
using UnityEngine;
using UnityEngine.UI;

namespace ADVance.Sample.Scenario
{
    // ヘッダー部のオート進行トグルを扱い、状態変化を通知する。
    public class ScenarioUIHeader : MonoBehaviour
    {
        [SerializeField] private Button _autoButton = null;
        private readonly Subject<bool> _onAutoModeChanged = new();
        public Observable<bool> OnAutoModeChanged => _onAutoModeChanged;
        [SerializeField] private Button _logButton = null;

        private readonly Subject<bool> _onShowLogChanged = new();
        public Observable<bool> OnShowLogChanged => _onShowLogChanged;

        private bool _isShowLog = false;
        private bool _isAutoModeEnabled;
        public bool IsAutoModeEnabled => _isAutoModeEnabled;

        public void Initialize()
        {
            SetEvent();
        }

        private void SetEvent()
        {
            _autoButton.OnClickAsObservable()
                .Subscribe(_ => ToggleAutoMode())
                .AddTo(this);
            _logButton.OnClickAsObservable()
                .Subscribe(_ => ToggleShowLog())
                .AddTo(this);
        }

        private void ToggleAutoMode()
        {
            SetAutoMode(!_isAutoModeEnabled);
        }

        private void ToggleShowLog()
        {
            _isShowLog = !_isShowLog;
            _onShowLogChanged.OnNext(_isShowLog);
        }

        private void SetAutoMode(bool isEnabled)
        {
            if (_isAutoModeEnabled == isEnabled)
            {
                return;
            }

            _isAutoModeEnabled = isEnabled;
            _onAutoModeChanged.OnNext(_isAutoModeEnabled);
        }

        private void OnDestroy()
        {
            _onAutoModeChanged.Dispose();
            _onShowLogChanged.Dispose();
        }
    }
}
