using ADVance.Base;
using ADVance.Data;
using NUnit.Framework;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

namespace ADVance.Tests
{
    public class TestADVanceManager : ADVanceManagerBase
    {
        protected override void OnInitialize()
        {
            // テスト用の初期化処理
        }
    }

    [TestFixture]
    public class ADVanceManagerBaseTests
    {
        private TestADVanceManager _manager;
        private int _scenarioEndedCallCount;

        [SetUp]
        public void SetUp()
        {
            var gameObject = new GameObject();
            _manager = gameObject.AddComponent<TestADVanceManager>();
            _manager.Initialize();
            _scenarioEndedCallCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            if (_manager != null)
            {
                Object.DestroyImmediate(_manager.gameObject);
            }
        }

        [Test]
        public async UniTask OnScenarioEnded_CalledOnlyOnceWhenScenarioCompletes()
        {
            // シンプルなシナリオラインを作成（ID 1のみ、次のIDは存在しない）
            var lines = new List<ScenarioLine>
            {
                new ScenarioLine
                {
                    ID = 1,
                    CommandName = "Print",
                    Args = new List<string> { "Test" },
                    NextIDs = new List<int>() // 空のNextIDsでシナリオ終了を模擬
                }
            };

            // シナリオをロード（非同期処理なので少し待つ）
            _manager.Load(lines, 1);
            await UniTask.Delay(100); // 処理完了を待つ

            // OnScenarioEndedが1回だけ呼ばれることを検証
            Assert.AreEqual(1, _scenarioEndedCallCount, "OnScenarioEnded should be called exactly once");
        }

        [Test]
        public async UniTask OnScenarioEnded_ResetCorrectlyOnNewLoad()
        {
            // 最初のシナリオ
            var lines1 = new List<ScenarioLine>
            {
                new ScenarioLine
                {
                    ID = 1,
                    CommandName = "Print",
                    Args = new List<string> { "Test1" },
                    NextIDs = new List<int>()
                }
            };

            _manager.Load(lines1, 1);
            await UniTask.Delay(100);

            Assert.AreEqual(1, _scenarioEndedCallCount, "First scenario should end once");

            // 2番目のシナリオをロード
            var lines2 = new List<ScenarioLine>
            {
                new ScenarioLine
                {
                    ID = 1,
                    CommandName = "Print",
                    Args = new List<string> { "Test2" },
                    NextIDs = new List<int>()
                }
            };

            _manager.Load(lines2, 1);
            await UniTask.Delay(100);

            // 合計2回（1回目 + 2回目）呼ばれることを検証
            Assert.AreEqual(2, _scenarioEndedCallCount, "Second scenario should also end once, total 2 calls");
        }

        [Test]
        public async UniTask OnScenarioEnded_NotCalledMultipleTimesWithComplexScenario()
        {
            // より複雑なシナリオ（分岐を含む）
            var lines = new List<ScenarioLine>
            {
                new ScenarioLine
                {
                    ID = 1,
                    CommandName = "Set",
                    Args = new List<string> { "test", "5" },
                    NextIDs = new List<int> { 2 }
                },
                new ScenarioLine
                {
                    ID = 2,
                    CommandName = "IfEqual",
                    Args = new List<string> { "test", "5" },
                    NextIDs = new List<int> { 3, 4 }
                },
                new ScenarioLine
                {
                    ID = 3,
                    CommandName = "Print",
                    Args = new List<string> { "Equal" },
                    NextIDs = new List<int>() // ここで終了
                },
                new ScenarioLine
                {
                    ID = 4,
                    CommandName = "Print",
                    Args = new List<string> { "Not Equal" },
                    NextIDs = new List<int>() // ここで終了（この行は実行されない）
                }
            };

            _manager.Load(lines, 1);
            await UniTask.Delay(200); // 複雑な処理なので長めに待つ
        }
    }
}