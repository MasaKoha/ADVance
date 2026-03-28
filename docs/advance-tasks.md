# ADVance タスクリスト
作成日: 2026-03-28

## 設計方針

**ADVance はドメイン非依存の汎用シナリオエンジン。**
ゲーム固有の概念（感情値・キャラクター名・ゲームイベント等）は一切含まない。
ゲーム固有の拡張は、各ゲームプロジェクト側で `IScenarioCommandAsync` を実装して登録する。

---

## P0（動作の前提条件）

### アーキテクチャ修正

- [ ] **`IScenarioCommandAsync` 設計修正：戻り値で NextID を制御する**
  - 現状: `ProceedAsync()` が `line.CommandName == "Branch"` を文字列判定して次IDを決定している（拡張禁止な設計）
  - 修正後: `ExecuteCommandAsync` が `int?` を返す。`null` = `NextIDs[0]`、`n` = `NextIDs[n]` へ遷移
  - `BranchCommand` は `true/false` を `0/1` にマップして返す
  - `ChoiceCommand` は選択インデックスを返す
  - `Manager` からコマンド種別の文字列判定ロジックを全て除去する

- [ ] **`CancellationToken` の伝搬**
  - `IScenarioCommandAsync.ExecuteCommandAsync(ScenarioLine, CancellationToken)` にシグネチャ変更
  - `ProceedAsync(CancellationToken)` にシグネチャ変更
  - `ADVanceManagerBase` に `Stop()` を追加し `CancellationTokenSource` をキャンセルする
  - `ProceedAsync().Forget()` を廃止し `Load()` 内でトークンを渡す

### 変数管理

- [ ] **変数ストア（`IScenarioVariableStore` + `InMemoryVariableStore`）**
  - `IScenarioVariableStore` インターフェース（`Set / TryGet`）
  - デフォルト実装: `Dictionary<string, string>` ベースの `InMemoryVariableStore`
  - 永続化・型変換はゲーム側が実装を差し替える DI 設計
  - `ADVanceManagerBase` にストアを保持し、コマンドが参照できるよう渡す

- [ ] **変数操作コマンド（`SetVarCommand`）**
  - `CommandName = "SetVar"`, `Args: [varName, value]`
  - 変数ストアへの書き込み

- [ ] **変数参照型の分岐評価器**
  - `IScenarioBranchEvaluator.Evaluate(args, IScenarioVariableStore)` にシグネチャ拡張
  - 引数を評価する際、`$varName` 形式を変数ストアの値で解決してから比較する
  - 既存の 6 種演算子（Equal/NotEqual/Greater 等）をストア参照対応に更新

### 入力待機

- [ ] **`IScenarioInputProvider` 抽象化**
  - テキスト送り・選択肢選択をインターフェースで抽象化
  ```csharp
  public interface IScenarioInputProvider
  {
      UniTask WaitForAdvanceAsync(CancellationToken cancellationToken);
      UniTask<int> WaitForChoiceAsync(IReadOnlyList<string> choices, CancellationToken cancellationToken);
  }
  ```
  - `ADVanceManagerBase` にコンストラクタ or `Initialize()` で注入

- [ ] **`ShowTextCommand` 実体化**
  - `IScenarioInputProvider.WaitForAdvanceAsync()` を `await` してから完了する
  - `IScenarioTextFormatter` でテキストの変数展開を行ってからコールバックに渡す
  - `ADVanceManagerBase.OnLineExecuted` に展開済みテキストを乗せる

- [ ] **`ChoiceCommand` 待機ロジック実装**
  - `IScenarioInputProvider.WaitForChoiceAsync()` を `await` して選択インデックスを取得
  - `ADVanceManagerBase.Select()` メソッドを `IScenarioInputProvider` の実装に移動して除去する
  - 選択インデックスを `ExecuteCommandAsync` の戻り値で返す（NextID 制御修正と連動）

---

## P1（実用シナリオに必要）

### 遷移・制御

- [ ] **ラベル管理**
  - `ScenarioLine` に `string Label` フィールドを追加（任意）
  - `Load()` 時にラベル名 → ID の逆引き辞書を構築
  - `CsvParser` / `CsvImporter` をラベル列に対応

- [ ] **`JumpCommand`（無条件ジャンプ）**
  - `CommandName = "Jump"`, `Args: [labelName or lineId]`
  - ラベル名を指定した場合は辞書から ID を解決
  - `ExecuteCommandAsync` の戻り値でジャンプ先 NextID インデックスを返す

- [ ] **`WaitCommand`（秒数待機）**
  - `CommandName = "Wait"`, `Args: [durationSeconds]`
  - `UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken)` を `await`

- [ ] **Pause / Resume API**
  - `ADVanceManagerBase.PauseAsync()` / `ResumeAsync()`
  - 中断時の `CurrentLineId` を外部から取得できるプロパティを追加

### イベント

- [ ] **ライフサイクルイベント追加**
  - `Observable<Unit> OnScenarioStarted`（`Load()` 呼び出し時）
  - `Observable<Unit> OnScenarioCompleted`（末尾ラインの NextID が空になったとき）
  - `Debug.Log("シナリオ終了")` の直書きを除去して `OnScenarioCompleted` に置き換える
  - `Observable<ScenarioLine> OnBeforeLineExecuted`（コマンド実行前フック）

### 外部連携

- [ ] **`ExternalEventCommand`（コールバック注入）**
  - `CommandName = "Event"`, `Args: [eventName, ...extraArgs]`
  - ゲーム側が `RegisterEventHandler(string eventName, Func<List<string>, CancellationToken, UniTask> handler)` で登録
  - ADVance 本体はイベント名とハンドラの Registry を持つだけ。中身はゲーム側が実装する
  - これにより感情値変化・画面遷移・エフェクト等のゲーム固有処理をシナリオ記述から発火できる

### データ

- [ ] **セーブ / ロード（`ScenarioSaveData`）**
  - `ScenarioSaveData { int CurrentLineId; Dictionary<string, string> Variables; }`
  - `ADVanceManagerBase.GetSaveData()` / `RestoreFromSaveData(ScenarioSaveData)` を公開
  - 永続化方法はゲーム側に委ねる。ADVance はデータクラスの提供のみ

### CSV 記述

- [ ] **コメント行対応**
  - `#` で始まる行をコメントとして `CsvParser` で読み飛ばす
  - ライター・非エンジニアがシナリオに意図・担当メモを残せる

---

## P2（品質・制作効率向上）

- [ ] **テキスト変数展開（`IScenarioTextFormatter`）**
  - `{key}` プレースホルダーを変数ストアの値で展開するインターフェース
  - デフォルト実装 `DefaultTextFormatter` を同梱
  - ルビ・タグ等の独自構文はゲーム側が実装を差し替え可能

- [ ] **コマンドミドルウェア（`IScenarioCommandMiddleware`）**
  - コマンド実行前後にフックを差し込める chain-of-responsibility 設計
  - ログ出力・オートモードのウェイト挿入・タイムアウト監視に使う

- [ ] **コマンド並列実行（`ParallelCommand`）**
  - `CommandName = "Parallel"`, `Args: [commandName1, commandName2, ...]`
  - `UniTask.WhenAll` で全コマンドの完了を待機
  - 「テキスト表示と BGM フェードイン同時」のような演出に対応

- [ ] **`CallCommand` / `ReturnCommand`（サブシナリオ呼び出し）**
  - 別 ScenarioData を読み込んで実行し、完了後に元の位置へ戻る
  - 繰り返し使う演出パターンをサブシナリオとして分離する
  - `_callStack` (Stack&lt;int&gt;) を Manager に保持

- [ ] **既読管理（Read History）**
  - 実行済み `LineId` の HashSet を Manager が保持
  - `IsRead(int lineId)` を公開
  - 既読スキップ・高速送りの実装基盤として使う

- [ ] **バックログ（Message Log）**
  - 実行済み `ScenarioLine` のリストを一定件数保持
  - `IReadOnlyList<ScenarioLine> GetMessageLog()` を公開
  - UI 側がバックログ画面を描画するためのデータソース
