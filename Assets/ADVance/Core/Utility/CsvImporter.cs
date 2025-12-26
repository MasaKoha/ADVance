using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using ADVance.Data;
using ADVance.Base;

namespace ADVance.Utility
{
    public static class CsvImporter
    {
        public static ScenarioData ImportFromCsv(TextAsset csvFile, ScenarioCommandRegistry commandRegistry = null)
        {
            var scenarioData = ScriptableObject.CreateInstance<ScenarioData>();
            scenarioData.Lines = new List<ScenarioLine>();

            using var reader = new StringReader(csvFile.text);
            var isFirstLine = true;
            var lineIndex = 1; // 1から始まるIDを自動生成
            var errorMessages = new List<string>();

            while (reader.ReadLine() is { } line)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                // コメント行をスキップ（##や//で始まる行）
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("##") || trimmedLine.StartsWith("//") || string.IsNullOrEmpty(trimmedLine))
                {
                    continue;
                }

                var columns = CsvParser.ParseLine(line);
                var commandName = columns[0];

                // コマンド検証
                if (commandRegistry != null && !commandRegistry.HasCommand(commandName))
                {
                    errorMessages.Add($"Line {lineIndex}: Unknown command '{commandName}'");
                }

                // Taskコマンドの場合、第2引数（実行するコマンド名）も検証
                if (commandRegistry != null && commandName == "Task" && columns.Count > 3)
                {
                    var innerCommandName = columns[3]; // Task,NextIDs,tag,commandName の順序
                    if (!commandRegistry.HasCommand(innerCommandName))
                    {
                        errorMessages.Add($"Line {lineIndex}: Task command contains unknown inner command '{innerCommandName}'");
                    }
                }

                var scenarioLine = new ScenarioLine
                {
                    ID = lineIndex++, // 行番号を自動生成
                    CommandName = commandName, // 1列目はCommandName
                    NextIDs = ParseNextIDs(columns[1]), // 2列目はNextIDs
                    Args = new List<string>()
                };

                // 3列目以降を Args に格納
                for (var i = 2; i < columns.Count; i++)
                {
                    scenarioLine.Args.Add(columns[i]);
                }

                scenarioData.Lines.Add(scenarioLine);
            }

            // エラーがあれば例外を投げる
            if (errorMessages.Count > 0)
            {
                var errorMessage = string.Join("\n", errorMessages);
                throw new System.ArgumentException($"Invalid commands found in CSV:\n{errorMessage}");
            }

            return scenarioData;
        }

        private static List<int> ParseNextIDs(string nextIDs)
        {
            var result = new List<int>();
            if (string.IsNullOrEmpty(nextIDs))
            {
                // NextIDsが空の場合は1を設定
                result.Add(1);
                return result;
            }

            foreach (var id in nextIDs.Split('|'))
            {
                if (int.TryParse(id, out var parsedID))
                {
                    result.Add(parsedID);
                }
            }

            return result;
        }
    }
}