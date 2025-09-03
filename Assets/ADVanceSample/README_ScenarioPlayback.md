# シナリオ再生システム

## 概要
このシステムは、ScriptableObjectとして保存されたシナリオデータを読み込んで、シナリオを再生するためのものです。コマンドベースの設計により、各処理が独立したコマンドクラスで実装されています。

## アーキテクチャ

### コマンドパターンの採用
- 各シナリオコマンド（Show, Set, Print, Choice等）は独立したクラスとして実装
- ScenarioManagerは各コマンドのObservableを購読してUI更新を行う
- 処理の分離により、コマンドの追加・修正が容易

### 主要コンポーネント
- **ADVanceManagerBase**: シナリオの実行制御とコマンド管理
- **ScenarioCommandRegistry**: コマンドの登録と実行管理  
- **各Commandクラス**: 具体的な処理とObservableによるイベント通知
- **ScenarioManager**: UI制御とコマンドイベントの購読

## 必要なコンポーネント

### 1. ScenarioManager
- `Assets/ADVanceSample/ScenarioManager.cs`
- ADVanceManagerBaseを継承したコンポーネント

### 2. UI設定
以下のUI要素が必要です：

#### Canvas
- Canvas
- GraphicRaycaster
- CanvasScaler

#### Dialog Text
- Text コンポーネント
- シナリオのテキストを表示

#### Next Button
- Button コンポーネント
- テキスト表示後のクリック待ち用

#### Choice Panel
- GameObject（親オブジェクト）
- 選択肢表示時のみアクティブになる

#### Choice Buttons
- Button[] 配列（2個以上推奨）
- 各ボタンに子のTextコンポーネントが必要

## セットアップ手順

### 1. Unityシーンの作成
```
ScenarioPlaybackScene
├── Canvas
│   ├── DialogText (Text)
│   ├── NextButton (Button)
│   └── ChoicePanel (GameObject)
│       ├── ChoiceButton1 (Button)
│       │   └── Text
│       └── ChoiceButton2 (Button)
│           └── Text
└── ScenarioManager (GameObject)
```

### 2. ScenarioManagerの設定
1. 空のGameObjectを作成し、名前を「ScenarioManager」にする
2. ScenarioManagerコンポーネントをアタッチ
3. UI Referencesに以下を設定：
   - Dialog Text: DialogText
   - Choice Buttons: ChoiceButton1, ChoiceButton2の配列
   - Choice Panel: ChoicePanel
   - Next Button: NextButton
4. Scenario Dataに以下を設定：
   - Scenario Data: SimpleScenario.asset

### 3. シナリオデータの構造
SimpleScenario.assetに以下のシナリオデータが含まれています：

```
ID=1: Show "こんにちは、${name}さん！" → ID=2
ID=2: Set like=5 → ID=3  
ID=3: IfGreaterEqual like>=10 → ID=4(true) or ID=5(false)
ID=4: Show "好感度高いね！" → ID=6
ID=5: Show "まだ距離があるみたい。" → ID=6
ID=6: Choice mood ["元気！", "普通かな"] → ID=7 or ID=8
ID=7: Show "元気そうで安心した！" → ID=9
ID=8: Show "なるほど、落ち着いてるね。" → ID=9
ID=9: Print mood → 終了
```

## コマンドの説明

### Show
テキストを表示し、クリック待ちする
- Args[0]: 表示するテキスト（変数置換対応）

### Set
変数に値を設定する
- Args[0]: 変数名
- Args[1]: 値

### IfGreaterEqual
条件分岐（以上）
- Args[0]: 変数名
- Args[1]: 比較値
- NextIDs[0]: true時の次ID
- NextIDs[1]: false時の次ID

### Choice
選択肢を表示する
- Args[0]: 結果を格納する変数名
- Args[1以降]: 選択肢のテキスト

### Print
変数の値をログに出力し、テキストに表示する
- Args[0]: 変数名

## 使用方法

1. Unityでシーンを開く
2. Playボタンを押す
3. シナリオが自動的に開始される
4. テキスト表示時は「Next」ボタンをクリック
5. 選択肢表示時は好きな選択肢をクリック

## デバッグ情報

コンソールに以下の情報が表示されます：
- 実行中のコマンド
- 変数の設定値
- 分岐条件の結果
- 選択した選択肢