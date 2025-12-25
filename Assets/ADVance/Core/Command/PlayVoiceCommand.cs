using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class PlayVoiceCommand : CommandBase
    {
        public override string CommandName => "PlayVoice";
        private Subject<PlayVoiceParameter> _onPlayVoice;
        public Observable<PlayVoiceParameter> OnPlayVoice => _onPlayVoice ??= new Subject<PlayVoiceParameter>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var fadeInTime = args.Count > 1 ? args[1].ToFloat() : 0f;
            var volume = args.Count > 2 ? args[2].ToFloat() : 0f;
            _onPlayVoice?.OnNext(new PlayVoiceParameter()
            {
                Key = key,
                FadeInTime = fadeInTime,
                Volume = volume
            });
            return UniTask.CompletedTask;
        }
    }

    public struct PlayVoiceParameter
    {
        public string Key;
        public float FadeInTime;
        public float Volume;
    }
}
