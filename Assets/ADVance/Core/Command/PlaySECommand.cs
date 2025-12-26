using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class PlaySECommand : CommandBase
    {
        public override string CommandName => "PlaySE";
        private Subject<PlaySEParameter> _onPlaySe;
        public Observable<PlaySEParameter> OnPlaySe => _onPlaySe ??= new Subject<PlaySEParameter>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var fadeInTime = args.Count > 1 ? args[1].ToFloat() : 0f;
            var volume = args.Count > 2 ? args[2].ToFloat() : 0f;
            _onPlaySe?.OnNext(new PlaySEParameter()
            {
                Key = key,
                FadeInTime = fadeInTime,
                Volume = volume
            });
            return UniTask.CompletedTask;
        }
    }

    public struct PlaySEParameter
    {
        public string Key;
        public float FadeInTime;
        public float Volume;
    }
}
