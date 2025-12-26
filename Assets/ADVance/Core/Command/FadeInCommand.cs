using System.Collections.Generic;
using ADVance.Utility;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class FadeInCommand : CommandBase
    {
        public override string CommandName => "FadeIn";
        private Subject<(FadeInParameter parameter, UniTaskCompletionSource completionSource)> _onFadeIn;
        public Observable<(FadeInParameter parameter, UniTaskCompletionSource completionSource)> OnFadeIn => _onFadeIn ??= new Subject<(FadeInParameter, UniTaskCompletionSource)>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var duration = args.Count > 0 ? args[0].ToFloat() : 0f;
            var colorCode = args.Count > 1 ? args[1] : "";
            var parameter = new FadeInParameter
            {
                Duration = duration,
                ColorCode = colorCode,
            };

            var completionSource = new UniTaskCompletionSource();
            _onFadeIn?.OnNext((parameter, completionSource));
            // アニメーション完了まで待機
            await completionSource.Task;
            Manager.SetNextLineId();
        }
    }

    public struct FadeInParameter
    {
        public float Duration;
        public string ColorCode;
    }
}
