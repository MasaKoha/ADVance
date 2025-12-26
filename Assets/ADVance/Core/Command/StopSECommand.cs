using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class StopSECommand : CommandBase
    {
        public override string CommandName => "StopSE";
        private Subject<StopSEParameter> _onStopSE;
        public Observable<StopSEParameter> OnStopSE => _onStopSE ??= new Subject<StopSEParameter>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var fadeOutTime = args.Count > 1 ? args[1].ToFloat() : 0f;
            var volume = args.Count > 2 ? args[2].ToFloat() : 0f;
            _onStopSE.OnNext(new StopSEParameter()
            {
                Key = key,
                FadeOutTime = fadeOutTime,
                Volume = volume
            });
            return UniTask.CompletedTask;
        }
    }

    public struct StopSEParameter
    {
        public string Key;
        public float FadeOutTime;
        public float Volume;
    }
}
