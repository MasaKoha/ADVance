using System.Collections.Generic;
using ADVance.Base;
using ADVance.Command;
using ADVance.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

namespace ADVance.Manager
{
    public class ScenarioManager : ADVanceManagerBase
    {
        protected override void OnInitialize()
        {
            SetupCore();
        }

        private void SetupCore()
        {
            // シナリオラインの実行を監視
            OnLineExecuted.Subscribe(OutPutLineLog).AddTo(destroyCancellationToken);
            // コマンドのObservableを購読
            SubscribeToCommands();
        }

        public void StartScenario(ScenarioData scenarioData)
        {
            if (scenarioData == null)
            {
                Debug.LogError("Scenario data is not assigned!");
                return;
            }

            Load(scenarioData.Lines);
        }

        private void OutPutLineLog(ScenarioLine line)
        {
            var args = line.Args != null ? string.Join(", ", line.Args) : "no args";
            Debug.Log($"Executing: {line.CommandName} - {args}");
        }

        private void SubscribeToCommands()
        {
            // SetCommandの購読
            var setCommand = CommandRegistry.GetCommand<SetCommand>();
            setCommand.OnVariableSet
                .Subscribe(varData => { Variables[varData.varName] = varData.value; })
                .AddTo(destroyCancellationToken);

            // PrintCommandの購読
            var printCommand = CommandRegistry.GetCommand<PrintCommand>();
            printCommand.OnPrint
                .Subscribe(varName => { Debug.Log($"{varName}: {Variables[varName]}"); })
                .AddTo(destroyCancellationToken);

            // ChoiceCommandの購読
            var choiceCommand = CommandRegistry.GetCommand<ChoiceCommand>();
            choiceCommand?.OnChoiceShow
                .Subscribe(ShowChoices)
                .AddTo(destroyCancellationToken);

            // PreloadAssetCommandの購読
            var preloadAssetCommand = CommandRegistry.GetCommand<RequestAssetCommand>();
            preloadAssetCommand.OnReserveLoadAsset
                .Subscribe(assetData => { AssetLoader.ReserveAssetLoad(assetData.Key, assetData.AssetPath); })
                .AddTo(destroyCancellationToken);

            // InitVariableCommandの購読
            var initVariableCommand = CommandRegistry.GetCommand<InitVariableCommand>();
            initVariableCommand.OnVariableInit
                .Subscribe(varData => { Variables[varData.varName] = varData.value; })
                .AddTo(destroyCancellationToken);

            // FinishCommandの購読
            var finishCommand = CommandRegistry.GetCommand<FinishCommand>();
            finishCommand.OnFinish
                .Subscribe(_ => { OnScenarioFinished(); })
                .AddTo(destroyCancellationToken);
        }

        private readonly Subject<bool> _onStartDownloadButtonInteractable = new();
        public Observable<bool> OnStartDownloadButtonInteractable => _onStartDownloadButtonInteractable;

        private string GetVariableValue(string varName)
        {
            return Variables.TryGetValue(varName, out var variable) ? variable.ToString() : "0";
        }

        private void ShowChoices(List<string> args)
        {
            if (args.Count < 2)
            {
                return;
            }

            var varName = args[0];
            var choices = args.GetRange(1, args.Count - 1);
            _onShowChoice.OnNext((choices, varName));
        }

        private readonly Subject<(List<string> choices, string varName)> _onShowChoice = new();
        public Observable<(List<string> choices, string varName)> OnShowChoice => _onShowChoice;

        public override void Select(int choiceIndex)
        {
            // 選択された選択肢の変数を設定
            var current = GetCurrentLine();
            if (current is { CommandName: "Choice" } && current.Args.Count > choiceIndex + 1)
            {
                var varName = current.Args[0];
                var selectedChoice = current.Args[choiceIndex + 1];
                Variables[varName] = selectedChoice;
            }

            base.Select(choiceIndex);
        }
    }
}