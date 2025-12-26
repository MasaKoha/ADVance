using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class ShowStillCommand : CommandBase
    {
        public override string CommandName => "ShowStill";
        private Subject<ShowStillParameter> _onShowStill;
        public Observable<ShowStillParameter> OnShowStill => _onShowStill ??= new Subject<ShowStillParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var still = args[0];
            var parameter = new ShowStillParameter
            {
                Key = still,
            };
            _onShowStill?.OnNext(parameter);
            Manager.SetNextLineId();
            await Manager.Wait();
        }
    }

    public struct ShowStillParameter
    {
        public string Key;
    }
}