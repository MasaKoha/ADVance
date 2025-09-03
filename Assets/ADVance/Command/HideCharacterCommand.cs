using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Command
{
    public class HideCharacterCommand : CommandBase
    {
        public override string CommandName => "HideCharacter";
        private Subject<string> _onHideCharacter;
        public Observable<string> OnHideCharacter => _onHideCharacter ??= new Subject<string>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var character = args[0];
            _onHideCharacter?.OnNext(character);
            await Manager.Wait();
        }
    }
}