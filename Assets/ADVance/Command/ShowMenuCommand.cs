using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class ShowMenuCommand : CommandBase
    {
        public override string CommandName => "ShowMenu";
        private Subject<Unit> _onShowMenu;
        public Observable<Unit> OnShowMenu => _onShowMenu ??= new Subject<Unit>();

        public override UniTask ExecuteCommandAsync(List<string> args)
        {
            _onShowMenu?.OnNext(Unit.Default);
            return UniTask.CompletedTask;
        }
    }
}