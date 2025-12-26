using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class SepiaCommand : CommandBase
    {
        public override string CommandName => "Sepia";
        private Subject<SepiaParameter> _onSepia;
        public Observable<SepiaParameter> OnSepia => _onSepia ??= new Subject<SepiaParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var enabled = false;
            bool.TryParse(args[0], out enabled);
            var parameter = new SepiaParameter
            {
                Enabled = enabled,
            };

            _onSepia?.OnNext(parameter);
            Manager.SetNextLineId();
            await Manager.Wait();
        }
    }

    public struct SepiaParameter
    {
        public bool Enabled;
    }
}