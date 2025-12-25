using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class ShowCharacterCommand : CommandBase
    {
        public override string CommandName => "ShowCharacter";
        private Subject<ShowCharacterParameter> _onShowCharacter;
        public Observable<ShowCharacterParameter> OnShowCharacter => _onShowCharacter ??= new Subject<ShowCharacterParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var character = args[0];
            var x = args.Count > 1 ? args[1].ToFloat() : 0f;
            var y = args.Count > 2 ? args[2].ToFloat() : 0f;
            var parameter = new ShowCharacterParameter
            {
                Key = character,
                X = x,
                Y = y
            };
            _onShowCharacter?.OnNext(parameter);
            Manager.SetNextLineId();
            await Manager.Wait();
        }
    }

    public struct ShowCharacterParameter
    {
        public string Key;
        public float X;
        public float Y;
    }
}
