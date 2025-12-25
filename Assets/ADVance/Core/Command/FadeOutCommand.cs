using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class FadeOutCommand : CommandBase
    {
        public override string CommandName => "FadeOut";
        private Subject<(FadeOutParameter parameter, UniTaskCompletionSource completionSource)> _onFadeOut;
        public Observable<(FadeOutParameter parameter, UniTaskCompletionSource completionSource)> OnFadeOut => _onFadeOut ??= new Subject<(FadeOutParameter, UniTaskCompletionSource)>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var duration = args.Count > 0 ? args[0].ToFloat() : 0f;
            var colorCode = args.Count > 1 ? args[1] : "";
            var parameter = new FadeOutParameter
            {
                Duration = duration,
                ColorCode = colorCode
            };

            var completionSource = new UniTaskCompletionSource();
            _onFadeOut?.OnNext((parameter, completionSource));
            // アニメーション完了まで待機
            await completionSource.Task;
            Manager.SetNextLineId();
        }
    }

    public struct FadeOutParameter
    {
        public float Duration;
        public string ColorCode;
    }
}
