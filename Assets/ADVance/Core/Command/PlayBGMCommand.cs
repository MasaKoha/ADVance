using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class PlayBGMCommand : CommandBase
    {
        public override string CommandName => "PlayBGM";
        private Subject<PlayBGMParameter> _onPlayBGM;
        public Observable<PlayBGMParameter> OnPlayBGM => _onPlayBGM ??= new Subject<PlayBGMParameter>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var fadeInTime = args.Count > 1 ? args[1].ToFloat() : 0f;
            var volume = args.Count > 2 ? args[2].ToFloat() : 0f;
            _onPlayBGM?.OnNext(new PlayBGMParameter()
            {
                Key = key,
                FadeInTime = fadeInTime,
                Volume = volume
            });
            return UniTask.CompletedTask;
        }
    }

    public struct PlayBGMParameter
    {
        public string Key;
        public float FadeInTime;
        public float Volume;
    }
}
