using UnityEngine;

namespace ADVance.Sample.Scenario
{
    /// <summary>
    /// ADV 演出全体に適用する進行スピード係数を管理する。
    /// </summary>
    public sealed class ScenarioSpeedController
    {
        private float _speedMultiplier = 1f;

        /// <summary>
        /// 進行スピード係数。1.0 が通常速度。
        /// </summary>
        public float SpeedMultiplier => _speedMultiplier;

        /// <summary>
        /// 進行スピード係数を設定する。0 以下を指定した場合は 0 として扱う（即時完了）。
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// 秒数ベースの処理時間をスピード係数に応じて補正する。
        /// </summary>
        public float AdjustSeconds(float seconds)
        {
            if (_speedMultiplier <= 0f)
            {
                return 0f;
            }

            return seconds / _speedMultiplier;
        }

        /// <summary>
        /// ミリ秒ベースの処理時間をスピード係数に応じて補正する。
        /// </summary>
        public int AdjustMilliseconds(int milliseconds)
        {
            if (_speedMultiplier <= 0f)
            {
                return 0;
            }

            return Mathf.Max(0, Mathf.RoundToInt(milliseconds / _speedMultiplier));
        }
    }
}