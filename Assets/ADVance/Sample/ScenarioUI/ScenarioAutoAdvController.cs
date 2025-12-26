using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Sample.Scenario
{
    // ADV のオート進行タイミングを管理し、必要に応じて一時停止できるブロッカーを提供する。
    public sealed class ScenarioAutoAdvController : IDisposable
    {
        private readonly int _autoAdvanceDelayMilliseconds;
        private ScenarioSpeedController _speedController = null;
        private readonly CompositeDisposable _disposables = new();
        private ScenarioUIHeader _header;
        private Action _requestAdvance;
        private CancellationTokenSource _autoModeCancellationTokenSource;
        private bool _isAutoModeEnabled;
        private bool _isAwaitingAdvance;
        private int _autoAdvanceBlockers;

        public ScenarioAutoAdvController(int autoAdvanceDelayMilliseconds = 2000)
        {
            _autoAdvanceDelayMilliseconds = autoAdvanceDelayMilliseconds;
        }

        public void Initialize(ScenarioUIHeader header, Action requestAdv, ScenarioSpeedController speedController)
        {
            _header = header;
            _requestAdvance = requestAdv;
            _speedController = speedController;
            SetAutoModeState(_header.IsAutoModeEnabled);
            SetEvent();
        }

        private void SetEvent()
        {
            _disposables.Add(_header.OnAutoModeChanged.Subscribe(SetAutoModeState));
            _disposables.Add(_header.OnShowLogChanged.Subscribe(SetShowLogState));
        }

        private void SetShowLogState(bool isShowLog)
        {
            if (!isShowLog)
            {
                return;
            }

            // ログ表示中はオート進行を停止する。
            SetAutoModeState(false);
        }

        public void SetAwaitingAdvance(bool isAwaiting)
        {
            _isAwaitingAdvance = isAwaiting;
        }

        public IDisposable BeginAutoAdvanceBlock()
        {
            return new AutoAdvanceBlock(this);
        }

        private void SetAutoModeState(bool isEnabled)
        {
            if (_isAutoModeEnabled == isEnabled)
            {
                return;
            }

            _isAutoModeEnabled = isEnabled;

            if (_isAutoModeEnabled)
            {
                StartAutoModeLoop();
            }
            else
            {
                StopAutoModeLoop();
            }
        }

        private void StartAutoModeLoop()
        {
            if (_autoModeCancellationTokenSource != null)
            {
                return;
            }

            _autoModeCancellationTokenSource = new CancellationTokenSource();
            AutoAdvLoopAsync(_autoModeCancellationTokenSource.Token).Forget();
        }

        private void StopAutoModeLoop()
        {
            if (_autoModeCancellationTokenSource == null)
            {
                return;
            }

            _autoModeCancellationTokenSource.Cancel();
            _autoModeCancellationTokenSource.Dispose();
            _autoModeCancellationTokenSource = null;
        }

        /// <summary>
        /// 通常のオートモードと同じ条件で進行の準備が整い次第、待ち時間なしで連続的に進行を要求する。
        /// </summary>
        public UniTask RunTurboAutoAsync(CancellationToken cancellationToken = default)
        {
            return RunTurboAutoInternalAsync(cancellationToken);
        }

        private async UniTask RunTurboAutoInternalAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!_isAwaitingAdvance)
                {
                    await UniTask.WaitUntil(() => _isAwaitingAdvance, cancellationToken: token);
                }

                if (HasAutoAdvanceBlocker())
                {
                    await UniTask.WaitUntil(() => !HasAutoAdvanceBlocker(), cancellationToken: token);
                }

                _requestAdvance?.Invoke();

                await UniTask.NextFrame(token);
            }
        }

        private async UniTask AutoAdvLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!_isAwaitingAdvance)
                    {
                        await UniTask.WaitUntil(() => _isAwaitingAdvance, cancellationToken: token);
                    }

                    if (HasAutoAdvanceBlocker())
                    {
                        await UniTask.WaitUntil(() => !HasAutoAdvanceBlocker(), cancellationToken: token);
                    }

                    var delayMs = _speedController.AdjustMilliseconds(_autoAdvanceDelayMilliseconds);
                    if (delayMs > 0)
                    {
                        await UniTask.Delay(delayMs, cancellationToken: token);
                    }
                    else
                    {
                        await UniTask.Yield(token);
                    }

                    if (!_isAutoModeEnabled || !_isAwaitingAdvance)
                    {
                        continue;
                    }

                    _requestAdvance?.Invoke();
                    await UniTask.NextFrame(token);
                }
            }
            catch (OperationCanceledException)
            {
                // Auto mode turned off or controller disposed.
            }
        }

        private bool HasAutoAdvanceBlocker()
        {
            return _autoAdvanceBlockers > 0;
        }

        private void AddAutoAdvanceBlocker()
        {
            _autoAdvanceBlockers++;
        }

        private void RemoveAutoAdvanceBlocker()
        {
            if (_autoAdvanceBlockers <= 0)
            {
                _autoAdvanceBlockers = 0;
                return;
            }

            _autoAdvanceBlockers--;
        }

        public void Dispose()
        {
            StopAutoModeLoop();
            _disposables.Dispose();
        }

        private sealed class AutoAdvanceBlock : IDisposable
        {
            private readonly ScenarioAutoAdvController _owner;
            private bool _isDisposed;

            public AutoAdvanceBlock(ScenarioAutoAdvController owner)
            {
                _owner = owner;
                _owner.AddAutoAdvanceBlocker();
            }

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
                _owner.RemoveAutoAdvanceBlocker();
            }
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedController.SetSpeedMultiplier(multiplier);
        }
    }
}
