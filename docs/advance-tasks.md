# ADVance タスクリスト
作成日: 2026-03-28

## 設計方針

**ADVance はドメイン非依存の汎用シナリオエンジン。**
ゲーム固有の概念（感情値・キャラクター名・ゲームイベント等）は一切含まない。
ゲーム固有の拡張は、各ゲームプロジェクト側で `IScenarioCommandAsync` を実装して登録する。

---

## 実装済み機能（develop 時点）

<details>
<summary>一覧</summary>

- テキスト表示（ShowTextCommand）：タイプライター演出・`${var}` 変数展開・入力待ち
- 選択肢（ChoiceCommand）：`Manager.Select()` で待機解除
- 変数ストア（`Dictionary<string, object>`）：int/float/bool/string 型推論
- 変数操作：`InitVariable` / `Set` / `Add` / `Print`
- 条件分岐：`Branch` + `IfEqual` / `IfNotEqual` / `IfGreater` / `IfGreaterEqual` / `IfLess` / `IfLessEqual`
- フロー制御：`Wait` / `Call`（サブシナリオ） / `Finish`
- キャラクター：`ShowCharacter` / `HideCharacter` / `MoveCharacter`（※移動アニメ未実装）
- 背景・演出：`ShowBackground` / `ShowStill` / `ShowIcon` / `FadeIn` / `FadeOut` / `Gray` / `Sepia`
- サウンド：`PlayBGM` / `StopBGM` / `PlaySE` / `StopSE` / `PlayVoice`
- メニュー：`ShowMenu` / `HideMenu`
- 並列実行：`Task`（蓄積）+ `Exec`（並列 UniTask.WhenAll）
- アセット管理：`RequestAsset` / `RequestSprite` / `RequestSound` / `LoadAll`
- ライフサイクルイベント：`OnLineExecuted` / `OnChoiceSelected` / `OnScenarioEnded`
- Sample UI：タイプライター・選択肢ポップアップ・バックログ・オートアドバンス・スピード制御・ターボモード

</details>

---

## P0（バグ・クリティカルな設計問題）

- [ ] **`ProceedAsync()` に `CancellationToken` を伝搬する**
  - 現状: while ループにキャンセル手段がなく、MonoBehaviour 破棄後も走り続けるリスクがある
  - `ProceedAsync(CancellationToken)` にシグネチャ変更
  - `Load()` で `this.GetCancellationTokenOnDestroy()` を渡す
  - `IScenarioCommandAsync.ExecuteCommandAsync` にも `CancellationToken` を追加し全コマンドに伝搬する

- [ ] **`InitVariableCommand` のフロー制御バグを修正する**
  - 現状: `await Manager.Wait()` の後に `SetNextLineId()` しており、`Wait()` が `ProceedAsync()` を再起動した後に次IDを設定するため意図した位置より1行進む可能性がある
  - `SetNextLineId()` を `await Manager.Wait()` より前に移動する

- [ ] **`SetCommand` が変数ストアに直接書き込まない設計を修正する**
  - 現状: `SetCommand` は Subject 通知だけで `Manager.SetVariable()` を呼ばない。`ScenarioManager` を継承しないカスタムサブクラスでは `Set` が機能しない
  - `SetCommand` が `Manager.SetVariable()` を直接呼ぶよう修正する（`AddCommand` と同じ方式に統一）
  - `ScenarioManager.SubscribeToCommands()` 側の重複処理を削除する

- [ ] **`TaskRegistry` の `static` フィールドを除去する**
  - 現状: `ConcurrentDictionary` が static のためシーン再開・複数シナリオ実行時に前回の Task が残留するリスクがある
  - `ADVanceManagerBase` のインスタンスフィールドに変更し、ライフサイクルをインスタンスに紐付ける

---

## P1（機能不全・接続漏れ）

- [ ] **`MoveCharacterCommand` の移動アニメーションを実装する**
  - 現状: `Character.MoveCharacter()` が `Debug.Log` + `UniTask.CompletedTask` のスタブ。入力待ちループが即完了してしまっている
  - DOTween による `transform.DOMove(target, duration)` を実装し、アニメーション完了で `UniTaskCompletionSource.TrySetResult()` を呼ぶ

- [ ] **Sample UI のサウンド系 Observable を接続する**
  - 現状: `ScenarioController.SetCommandEvent()` で `PlayBGM` / `StopBGM` / `PlaySE` / `StopSE` / `PlayVoice` を一切購読していない
  - `ScenarioController` にサウンド管理コンポーネントを追加し、各 Observable を購読して実際に音を鳴らす

- [ ] **`AssetPreloadManager` をシナリオフローに接続する**
  - 現状: `ScenarioManager` / `ScenarioController` のどちらからも参照されておらず、プリロードが機能していない
  - `ScenarioController.StartScenario()` フローに `AssetPreloadManager.PreloadAssetsAsync()` を組み込む
  - `AssetPreloadManager._loadedAssets` と `AssetLoaderBase._loadedAssets` の二重管理を解消する

- [ ] **`OnAssetRegistryUpdated` を実際に発行する**
  - `ADVanceManagerBase` に宣言はあるが `OnNext` が呼ばれていない
  - `RequestAsset` 系コマンド実行後にレジストリ更新を通知する

- [ ] **`ChoicePopup` の選択肢ボタンを動的生成に変更する**
  - 現状: Inspector にプリセットした固定数の `List<Button>` に依存しており、選択肢数が上限を超えると余りが非表示になる
  - ボタンを `Instantiate` で動的生成し、選択肢数に応じてレイアウトを調整する

- [ ] **変数展開ロジックの重複を解消する**
  - 現状: `ShowTextCommand.ProcessVariables()` と `ScenarioController.ProcessVariables()` に同一ロジックが2か所あり展開が2回走る
  - `ADVanceManagerBase` に `FormatText(string template)` として一元化し、両者を委譲する

- [ ] **`ScenarioManager.Select()` オーバーライドの解釈ズレを解消する**
  - 基底クラス: `Choice` の全 Args を選択肢テキストとして扱う
  - `ScenarioManager`: `Args[0]` を変数名・`Args[1]` 以降を選択肢値として扱う
  - どちらかに統一し、CSV 仕様・`ChoiceCommand` の Args 設計と一致させる

- [ ] **`ScenarioScenePresenter.SetEvent()` を実装または削除する**
  - 空実装のまま役割不明。必要なら実装、不要なら削除する

- [ ] **`_onStartDownloadButtonInteractable` を削除する**
  - `ScenarioManager` に宣言のみで購読側も発行側もない未使用フィールド

---

## P2（品質・運用性向上）

- [ ] **`AssetLoaderBase._loadedAssets` を `Dictionary` に変更する**
  - 現状: `List<(string key, Object asset)>` で `GetAsset<T>(key)` が O(N) の線形探索
  - `Dictionary<string, Object>` に変更して O(1) 参照にする

- [ ] **NextIDs の「相対オフセット」仕様をドキュメント化・見直す**
  - 現状: `GetNextId()` が `line.ID + NextIDs[index]` として相対加算する仕様がコード上に暗黙的
  - `CsvImporter` と仕様書にドキュメントを追記する
  - 絶対 ID 方式への変更が有効か検討する

- [ ] **セーブ / ロード API を追加する**
  - `ScenarioSaveData { int CurrentLineId; Dictionary<string, object> Variables; }` を定義
  - `ADVanceManagerBase.GetSaveData()` / `RestoreFromSaveData(ScenarioSaveData)` を公開
  - 永続化方法はゲーム側に委ねる

- [ ] **既読管理 API を追加する**
  - 実行済み `LineId` の `HashSet` を `ADVanceManagerBase` が保持
  - `bool IsRead(int lineId)` を公開
  - 既読スキップ・高速送りの実装基盤として使う

- [ ] **タイプライター速度をコマンドで制御できるようにする**
  - 現状: `ScenarioUITextPresenter.CharactersPerSecond = 20f` がハードコード
  - `SetSpeed` コマンドまたは `ShowText` のオプション引数として速度を指定できるようにする

- [ ] **コアのユニットテストを追加する**
  - 現状: ユーティリティ層（CsvParser / CsvImporter / ConvertValues / ScenarioBranchRegistry）のみカバー
  - 変数ストア読み書き・`Set` / `Add` / `InitVariable` の動作・`Branch` 分岐評価をテスト対象に追加する
  - コマンドの MonoBehaviour 依存を整理し、EditMode テスト可能な設計に近づける

- [ ] **CSV にコメント行を追加する**
  - `#` で始まる行を `CsvParser` でスキップする
  - ライターや非エンジニアが意図・担当メモをシナリオに残せるようにする
