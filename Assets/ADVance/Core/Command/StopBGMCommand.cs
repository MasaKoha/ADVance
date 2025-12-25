using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class StopBGMCommand : CommandBase
    {
        public override string CommandName => "StopBGM";
        private Subject<StopBGMParameter> _onStopBGM;
        public Observable<StopBGMParameter> OnStopBGM => _onStopBGM ??= new Subject<StopBGMParameter>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var fadeOutTime = args.Count > 1 ? args[1].ToFloat() : 0f;
            var volume = args.Count > 2 ? args[2].ToFloat() : 0f;
            _onStopBGM.OnNext(new StopBGMParameter()
            {
                Key = key,
                FadeOutTime = fadeOutTime,
                Volume = volume
            });
            return UniTask.CompletedTask;
        }
    }

    public struct StopBGMParameter
    {
        public string Key;
        public float FadeOutTime;
        public float Volume;
    }
}
