using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ADVance.Utility
{
    public static class CsvParser
    {
        public static List<List<string>> Parse(string csvText)
        {
            var result = new List<List<string>>();
            using var reader = new StringReader(csvText);
            while (reader.ReadLine() is { } line)
            {
                result.Add(ParseLine(line));
            }

            return result;
        }

        private static List<string> ParseLine(string line)
        {
            var fields = new List<string>();
            var stringBuilder = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '\"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '\"')
                        {
                            stringBuilder.Append('\"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }
                else
                {
                    switch (c)
                    {
                        case ',':
                            fields.Add(stringBuilder.ToString());
                            stringBuilder.Clear();
                            break;
                        case '\"':
                            inQuotes = true;
                            break;
                        default:
                            stringBuilder.Append(c);
                            break;
                    }
                }
            }

            fields.Add(stringBuilder.ToString());
            return fields;
        }
    }
}