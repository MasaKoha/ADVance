using System.Collections.Generic;
using System.Threading;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Command
{
    public class WaitCommand : CommandBase
    {
        private static CancellationTokenSource _currentWaitCancellation;
        private Subject<Unit> _onWaitStart;
        public Observable<Unit> OnWaitStart => _onWaitStart ??= new Subject<Unit>();

        public override string CommandName => "Wait";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var waitTime = args.Count > 0 ? args[0].ToFloat(float.NaN) : float.NaN;
            if (!float.IsNaN(waitTime) || string.Equals(args.Count > 0 ? args[0]?.Trim() : null, "NaN", System.StringComparison.OrdinalIgnoreCase))
            {
                if (waitTime > 0)
                {
                    // 前回のWait処理をキャンセル（重複実行を防ぐ）
                    _currentWaitCancellation?.Cancel();
                    _currentWaitCancellation = new CancellationTokenSource();

                    var cancellationToken = _currentWaitCancellation.Token;

                    // ユーザー入力待機を開始
                    Manager.SetWaitingForInput(true);

                    // Wait開始イベントを発行
                    _onWaitStart?.OnNext(Unit.Default);

                    try
                    {
                        // 指定時間待機（キャンセル可能）
                        await UniTask.Delay((int)(waitTime * 1000), cancellationToken: cancellationToken);
                    }
                    catch (System.OperationCanceledException)
                    {
                        // スキップされた場合（キャンセルされた場合）
                    }
                    finally
                    {
                        // 現在のWait処理の参照をクリア
                        if (_currentWaitCancellation != null && !_currentWaitCancellation.IsCancellationRequested)
                        {
                            _currentWaitCancellation = null;
                        }

                        // 入力待機を終了
                        Manager.SetWaitingForInput(false);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Invalid wait time: {(args.Count > 0 ? args[0] : "")}");
            }

            Manager.SetNextLineId();
        }

        // 外部からWaitをスキップするためのメソッド
        public static void SkipCurrentWait()
        {
            _currentWaitCancellation?.Cancel();
        }
    }
}
