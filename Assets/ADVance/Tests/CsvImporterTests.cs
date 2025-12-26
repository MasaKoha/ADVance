using System;
using System.Collections.Generic;
using ADVance.Command;
using ADVance.Utility;
using NUnit.Framework;
using UnityEngine;

namespace ADVance.Tests
{
    public class CsvImporterTests
    {
        [Test]
        public void ImportFromCsv_BuildsScenarioLines()
        {
            var csv = string.Join("\n", new[]
            {
                "Command,NextIDs,Arg1,Arg2",
                "Print,,hello",
                "Set,2|3,var,1",
                "## comment",
                "// comment",
                "",
                "Print,,world"
            });

            var registry = new ScenarioCommandRegistry();
            registry.Register(new PrintCommand());
            registry.Register(new SetCommand());

            var data = CsvImporter.ImportFromCsv(new TextAsset(csv), registry);

            Assert.AreEqual(3, data.Lines.Count);

            var first = data.Lines[0];
            Assert.AreEqual(1, first.ID);
            Assert.AreEqual("Print", first.CommandName);
            CollectionAssert.AreEqual(new List<int> { 1 }, first.NextIDs);
            CollectionAssert.AreEqual(new List<string> { "hello" }, first.Args);

            var second = data.Lines[1];
            Assert.AreEqual(2, second.ID);
            Assert.AreEqual("Set", second.CommandName);
            CollectionAssert.AreEqual(new List<int> { 2, 3 }, second.NextIDs);
            CollectionAssert.AreEqual(new List<string> { "var", "1" }, second.Args);

            var third = data.Lines[2];
            Assert.AreEqual(3, third.ID);
            Assert.AreEqual("Print", third.CommandName);
            CollectionAssert.AreEqual(new List<string> { "world" }, third.Args);
        }

        [Test]
        public void ImportFromCsv_UnknownCommand_Throws()
        {
            var csv = string.Join("\n", new[]
            {
                "Command,NextIDs,Arg1",
                "Unknown,,value"
            });

            var registry = new ScenarioCommandRegistry();
            registry.Register(new PrintCommand());

            var ex = Assert.Throws<ArgumentException>(() => CsvImporter.ImportFromCsv(new TextAsset(csv), registry));
            StringAssert.Contains("Unknown command", ex.Message);
        }

        [Test]
        public void ImportFromCsv_TaskInnerCommandUnknown_Throws()
        {
            var csv = string.Join("\n", new[]
            {
                "Command,NextIDs,Tag,CommandName,Arg1",
                "Task,,test,Missing,arg"
            });

            var registry = new ScenarioCommandRegistry();
            registry.Register(new TaskCommand());
            registry.Register(new PrintCommand());

            var ex = Assert.Throws<ArgumentException>(() => CsvImporter.ImportFromCsv(new TextAsset(csv), registry));
            StringAssert.Contains("Task command contains unknown inner command", ex.Message);
        }
    }
}
