using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class GrayCommand : CommandBase
    {
        public override string CommandName => "Gray";
        private Subject<GrayParameter> _onGray;
        public Observable<GrayParameter> OnGray => _onGray ??= new Subject<GrayParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var enabled = args.Count > 0 ? args[0].ToBool() : false;
            var parameter = new GrayParameter
            {
                Enabled = enabled,
            };

            _onGray?.OnNext(parameter);
            Manager.SetNextLineId();
            await Manager.Wait();
        }
    }

    public struct GrayParameter
    {
        public bool Enabled;
    }
}
