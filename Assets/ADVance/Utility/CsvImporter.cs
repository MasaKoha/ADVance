using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ADVance.Data;

namespace ADVance.Utility
{
    public static class CsvImporter
    {
        public static ScenarioData ImportFromCsv(TextAsset csvFile)
        {
            var scenarioData = ScriptableObject.CreateInstance<ScenarioData>();
            scenarioData.Lines = new List<ScenarioLine>();

            using var reader = new StringReader(csvFile.text);
            var isFirstLine = true;

            while (reader.ReadLine() is { } line)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var columns = CsvParser.ParseLine(line);

                var scenarioLine = new ScenarioLine
                {
                    ID = int.Parse(columns[0]),
                    CommandName = columns[1],
                    NextIDs = ParseNextIDs(columns[2]),
                    Args = new List<string>()
                };

                // 4列目以降を Args に格納
                for (var i = 3; i < columns.Count; i++)
                {
                    scenarioLine.Args.Add(columns[i]);
                }

                scenarioData.Lines.Add(scenarioLine);
            }

            return scenarioData;
        }

        private static List<int> ParseNextIDs(string nextIDs)
        {
            var result = new List<int>();
            if (string.IsNullOrEmpty(nextIDs))
            {
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