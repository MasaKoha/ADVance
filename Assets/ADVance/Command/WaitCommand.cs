using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ADVance.Command
{
    public class WaitCommand : CommandBase
    {
        private static CancellationTokenSource _currentWaitCancellation;
        
        public override string CommandName => "Wait";

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            if (float.TryParse(args[0], out var waitTime))
            {
                if (waitTime > 0)
                {
                    // 前回のWait処理をキャンセル（重複実行を防ぐ）
                    _currentWaitCancellation?.Cancel();
                    _currentWaitCancellation = new CancellationTokenSource();
                    
                    var cancellationToken = _currentWaitCancellation.Token;
                    
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
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Invalid wait time: {args[0]}");
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