using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class ShowIconCommand : CommandBase
    {
        public override string CommandName => "ShowIcon";
        private Subject<ShowIconParameter> _onShowIcon;
        public Observable<ShowIconParameter> OnShowIcon => _onShowIcon ??= new Subject<ShowIconParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var icon = args[0];
            var x = args.Count > 1 ? args[1].ToFloat() : 0f;
            var y = args.Count > 2 ? args[2].ToFloat() : 0f;
            var parameter = new ShowIconParameter
            {
                Key = icon,
                X = x,
                Y = y
            };
            _onShowIcon?.OnNext(parameter);
            Manager.SetNextLineId();
            await Manager.Wait();
        }
    }

    public struct ShowIconParameter
    {
        public string Key;
        public float X;
        public float Y;
    }
}
