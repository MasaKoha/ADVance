using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class ChoiceCommand : CommandBase
    {
        public override string CommandName => "Choice";
        private Subject<List<string>> _onChoiceShow;
        public Observable<List<string>> OnChoiceShow => _onChoiceShow ??= new Subject<List<string>>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            _onChoiceShow?.OnNext(args);
            
            // 選択待ち状態に設定
            Manager.SetWaitingForInput(true);
            
            // 選択されるまで待機
            while (Manager.IsWaitingForInput)
            {
                await UniTask.Yield();
            }
            
            // Wait()を呼んでProceedAsyncの継続を管理する
            await Manager.Wait();
        }
    }
}