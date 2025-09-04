using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace ADVance.Command
{
    public class ShowTextCommand : CommandBase
    {
        public override string CommandName => "ShowText";
        private Subject<ShowTextParameter> _onTextShow;
        public Observable<ShowTextParameter> OnTextShow => _onTextShow ??= new Subject<ShowTextParameter>();

        public override async UniTask ExecuteCommandAsync(List<string> args)
        {
            var speakerArg = args.Count > 0 ? args[0] : "";
            var textArg = args.Count > 1 ? args[1] : "";

            // ${varname}形式の変数を展開
            var speaker = ProcessVariables(speakerArg);
            var text = ProcessVariables(textArg);
            var parameter = new ShowTextParameter
            {
                Speaker = speaker,
                Text = text
            };
            _onTextShow?.OnNext(parameter);

            Manager.SetNextLineId();
            Manager.SetWaitingForInput(true);
            while (Manager.IsWaitingForInput)
            {
                await UniTask.Yield();
            }

            await Manager.Wait();
        }

        private string ProcessVariables(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = text;
            foreach (var kvp in Manager.Variables)
            {
                var placeholder = "${" + kvp.Key + "}";
                if (result.Contains(placeholder))
                {
                    result = result.Replace(placeholder, kvp.Value.ToString());
                }
            }

            return result;
        }
    }

    public struct ShowTextParameter
    {
        public string Speaker;
        public string Text;
    }
}