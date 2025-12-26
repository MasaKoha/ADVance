using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class HideCharacterCommand : CommandBase
    {
        public override string CommandName => "HideCharacter";
        private Subject<HideCharacterParameter> _onHideCharacter;
        public Observable<HideCharacterParameter> OnHideCharacter => _onHideCharacter ??= new Subject<HideCharacterParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var character = args[0];
            var parameter = new HideCharacterParameter
            {
                Key = character
            };
            _onHideCharacter.OnNext(parameter);
            Manager.SetNextLineId();
            await Manager.Wait();
        }
    }

    public struct HideCharacterParameter
    {
        public string Key;
    }
}