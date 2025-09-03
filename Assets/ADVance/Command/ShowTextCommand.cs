using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ADVance.Command
{
    public class ShowTextCommand : CommandBase
    {
        public override string CommandName => "ShowText";
        private Subject<string> _onTextShow;
        public Observable<string> OnTextShow => _onTextShow ??= new Subject<string>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            if (args.Count <= 0)
            {
                Manager.SetCurrentId(Manager.GetNextLineId());
                await Manager.Wait();
                return;
            }

            var text = args[0];
            _onTextShow?.OnNext(text);

            var nextId = Manager.GetNextLineId();
            Manager.SetWaitingForInput(true);
            Manager.SetCurrentId(nextId);

            // 入力待ち
            while (Manager.IsWaitingForInput)
            {
                await UniTask.Yield();
            }

            await Manager.Wait();
        }
    }
}