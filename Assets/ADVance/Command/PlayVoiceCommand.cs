using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Command
{
    public class PlayVoiceCommand : CommandBase
    {
        public override string CommandName => "PlayVoice";

        private Subject<string> _onPlayVoice;
        public Observable<string> OnPlayVoice => _onPlayVoice ??= new Subject<string>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            var bgmPath = args[0];
            _onPlayVoice?.OnNext(bgmPath);
            return UniTask.CompletedTask;
        }
    }
}