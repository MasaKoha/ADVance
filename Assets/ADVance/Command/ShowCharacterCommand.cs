using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Command
{
    public class ShowCharacterCommand : CommandBase
    {
        public override string CommandName => "ShowCharacter";

        private Subject<string> _onShowCharacter;
        public Observable<string> OnShowCharacter => _onShowCharacter ??= new Subject<string>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var character = args[0];
            _onShowCharacter?.OnNext(character);
            await Manager.Wait();
        }
    }
}