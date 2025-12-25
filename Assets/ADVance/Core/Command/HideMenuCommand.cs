using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class HideMenuCommand : CommandBase
    {
        public override string CommandName => "HideMenu";
        private Subject<Unit> _onHideMenu;
        public Observable<Unit> OnHideMenu => _onHideMenu ??= new Subject<Unit>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            _onHideMenu?.OnNext(Unit.Default);
            return UniTask.CompletedTask;
        }
    }
}