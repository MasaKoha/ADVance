using ADVance.Command.Operator;
using NUnit.Framework;
using System.Collections.Generic;

namespace ADVance.Tests
{
    [TestFixture]
    public class ScenarioBranchRegistryTests
    {
        private ScenarioBranchRegistry _registry;

        [SetUp]
        public void SetUp()
        {
            _registry = new ScenarioBranchRegistry();
            _registry.Register(new EqualCommand());
            _registry.Register(new NotEqualCommand());
            _registry.Register(new GreaterCommand());
            _registry.Register(new GreaterOrEqualCommand());
            _registry.Register(new LessCommand());
            _registry.Register(new LessOrEqualCommand());
        }

        [TestCase("Equal", new[] { "1", "1" }, true)]
        [TestCase("Equal", new[] { "1", "2" }, false)]
        [TestCase("NotEqual", new[] { "1", "2" }, true)]
        [TestCase("NotEqual", new[] { "1", "1" }, false)]
        [TestCase("Greater", new[] { "2", "1" }, true)]
        [TestCase("Greater", new[] { "1", "2" }, false)]
        [TestCase("GreaterOrEqual", new[] { "2", "2" }, true)]
        [TestCase("GreaterOrEqual", new[] { "3", "2" }, true)]
        [TestCase("GreaterOrEqual", new[] { "1", "2" }, false)]
        [TestCase("Less", new[] { "1", "2" }, true)]
        [TestCase("Less", new[] { "2", "1" }, false)]
        [TestCase("LessOrEqual", new[] { "1", "2" }, true)]
        [TestCase("LessOrEqual", new[] { "2", "2" }, true)]
        [TestCase("LessOrEqual", new[] { "3", "2" }, false)]
        public void Evaluate_Operator_ReturnsExpected(string op, string[] args, bool expected)
        {
            var result = _registry.Evaluate(op, new List<string>(args));
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Evaluate_UnknownOperator_ReturnsFalse()
        {
            var result = _registry.Evaluate("Unknown", new List<string> { "1", "1" });
            Assert.IsFalse(result);
        }

        [Test]
        public void RegisterBranch_RegistersEvaluator_CanRetrieveEvaluator()
        {
            var evaluator = new EqualCommand();
            var registry = new ScenarioBranchRegistry();
            registry.Register(evaluator);
            var retrieved = registry.GetEvaluator(evaluator.OperatorName);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(evaluator.GetType(), retrieved.GetType());
        }
    }
}