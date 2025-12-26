using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class MoveCharacterCommand : CommandBase
    {
        public override string CommandName => "MoveCharacter";
        private Subject<MoveCharacterParameter> _onCharacterMoved;
        public Observable<MoveCharacterParameter> OnCharacterMoved => _onCharacterMoved ??= new Subject<MoveCharacterParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var key = args[0];
            var targetX = args.Count > 1 ? args[1].ToFloat() : 0f;
            var targetY = args.Count > 2 ? args[2].ToFloat() : 0f;
            var duration = args.Count > 3 ? args[3].ToFloat() : 0f;
            var parameters = new MoveCharacterParameter
            {
                Key = key,
                TargetX = targetX,
                TargetY = targetY,
                Duration = duration
            };

            _onCharacterMoved?.OnNext(parameters);
            Manager.SetWaitingForInput(true);
            while (Manager.IsWaitingForInput)
            {
                await UniTask.Yield();
            }

            Manager.SetNextLineId();
        }
    }

    public struct MoveCharacterParameter
    {
        public string Key;
        public float TargetX;
        public float TargetY;
        public float Duration;
    }
}
