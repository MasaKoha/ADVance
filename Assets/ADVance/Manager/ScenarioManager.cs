using System.Collections.Generic;
using ADVance.Base;
using ADVance.Command;
using ADVance.Data;
using ADVance.Manager.Interface;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

namespace ADVance.Manager
{
    public class ScenarioManager : ADVanceManagerBase
    {
        [SerializeField] private AssetPreloadManager _assetPreloadManager;

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
            // アセットプリロードマネージャーの購読
            SetupAssetPreloadSubscriptions();
        }

        public async UniTask StartScenarioAsync(ScenarioData scenarioData)
        {
            if (scenarioData == null)
            {
                Debug.LogError("Scenario data is not assigned!");
                return;
            }

            Load(scenarioData.Lines);
            if (AssetRegistry.PreloadAssets.Count > 0)
            {
                await ShowPreloadConfirmation();
            }
            else
            {
                // アセットがない場合はそのまま開始
                Debug.Log("No assets to preload. Starting scenario immediately.");
            }
        }

        private async UniTask ShowPreloadConfirmation()
        {
            if (_assetPreloadManager == null)
            {
                Debug.LogWarning("AssetPreloadManager is not assigned!");
                return;
            }

            // アセットサイズを計算
            await AssetRegistry.UpdateAssetSizes((IAssetPreloadManager)_assetPreloadManager);
            _onShowAssetSize.OnNext((AssetRegistry.TotalEstimatedSize, AssetRegistry.PreloadAssets.Count));
        }

        private readonly Subject<(long size, int count)> _onShowAssetSize = new();
        public Observable<(long size, int count)> OnShowAssetSize => _onShowAssetSize;

        private void OutPutLineLog(ScenarioLine line)
        {
            Debug.Log($"Executing: {line.CommandName} - {string.Join(", ", line.Args)}");
        }

        private void SubscribeToCommands()
        {
            // SetCommandの購読
            var setCommand = CommandRegistry.GetCommand<SetCommand>();
            setCommand.OnVariableSet.Subscribe(varData => { Variables[varData.varName] = varData.value; }).AddTo(destroyCancellationToken);

            // PrintCommandの購読
            var printCommand = CommandRegistry.GetCommand<PrintCommand>();
            printCommand.OnPrint.Subscribe(varName => { Debug.Log($"{varName}: {Variables[varName]}"); }).AddTo(destroyCancellationToken);

            // ChoiceCommandの購読
            var choiceCommand = CommandRegistry.GetCommand<ChoiceCommand>();
            choiceCommand?.OnChoiceShow.Subscribe(ShowChoices).AddTo(destroyCancellationToken);

            // PreloadAssetCommandの購読
            var preloadAssetCommand = CommandRegistry.GetCommand<PreloadAssetCommand>();
            preloadAssetCommand.OnAssetRegistered.Subscribe(assetData =>
            {
                AssetRegistry.AddAsset(assetData);
                OnAssetRegistryUpdated.OnNext(AssetRegistry);
            }).AddTo(destroyCancellationToken);

            // InitVariableCommandの購読
            var initVariableCommand = CommandRegistry.GetCommand<InitVariableCommand>();
            initVariableCommand.OnVariableInit.Subscribe(varData => { Variables[varData.varName] = varData.value; }).AddTo(destroyCancellationToken);
        }

        public Observable<float> OnChangedDownloadProgress => _assetPreloadManager.OnDownloadProgress;

        private void SetupAssetPreloadSubscriptions()
        {
            // 全アセットダウンロード完了の監視
            _assetPreloadManager.OnAllAssetsDownloaded.Subscribe(success =>
            {
                if (success)
                {
                    Debug.Log("All assets downloaded successfully. Starting scenario...");
                }
                else
                {
                    Debug.LogWarning("Some assets failed to download. Starting scenario anyway...");
                }
            }).AddTo(destroyCancellationToken);
        }

        private readonly Subject<bool> _onStartDownloadButtonInteractable = new();
        public Observable<bool> OnStartDownloadButtonInteractable => _onStartDownloadButtonInteractable;

        public async UniTask StartAssetDownload()
        {
            _onStartDownloadButtonInteractable.OnNext(false);
            try
            {
                await _assetPreloadManager.PreloadAssetsAsync(AssetRegistry);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during asset preloading: {e.Message}");
            }
            finally
            {
                _onStartDownloadButtonInteractable.OnNext(true);
            }
        }

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