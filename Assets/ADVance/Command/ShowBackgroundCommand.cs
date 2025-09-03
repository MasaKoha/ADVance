using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Command
{
    public class ShowBackgroundCommand : CommandBase
    {
        public override string CommandName => "ShowBackground";

        private Subject<string> _onShowBackground;
        public Observable<string> OnShowBackground => _onShowBackground ??= new Subject<string>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var backgroundPath = args[0];
            _onShowBackground?.OnNext(backgroundPath);
            await UniTask.CompletedTask;
        }
    }
}