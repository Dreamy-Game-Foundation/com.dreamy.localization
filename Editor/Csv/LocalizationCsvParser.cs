using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Dreamy.Localization.Editor.Csv
{
    public static class LocalizationCsvParser
    {
        private static readonly HashSet<string> ReservedColumns =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "schema",
                "table",
                "key",
                "id",
                "source",
                "status",
                "smart",
                "asset_type",
                "preload",
                "shared_comment",
                "tags",
                "context",
            };

        public static LocalizationCsvDocument ParseFile(string path)
        {
            return Parse(File.ReadAllText(path, new UTF8Encoding(false)));
        }

        public static LocalizationCsvDocument Parse(string content)
        {
            var records = ReadRecords(content ?? string.Empty);
            if (records.Count == 0)
            {
                return new LocalizationCsvDocument();
            }

            var headers = records[0].Fields
                .Select(value => value.Trim().TrimStart('\uFEFF'))
                .ToList();
            var localeColumns = headers
                .Where(IsLocaleColumn)
                .ToList();
            var rows = new List<LocalizationCsvRow>();

            for (var recordIndex = 1; recordIndex < records.Count; recordIndex++)
            {
                var record = records[recordIndex];
                if (record.Fields.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                var values = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);
                for (var column = 0; column < headers.Count; column++)
                {
                    if (!values.ContainsKey(headers[column]))
                    {
                        values.Add(
                            headers[column],
                            column < record.Fields.Count
                                ? record.Fields[column]
                                : string.Empty);
                    }
                }

                var localizedValues = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);
                var localizedComments = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var localeColumn in localeColumns)
                {
                    localizedValues[localeColumn] =
                        GetValue(values, localeColumn);
                    localizedComments[localeColumn] =
                        GetValue(values, $"{localeColumn}_comment");
                }

                rows.Add(new LocalizationCsvRow
                {
                    SourceLine = record.Line,
                    SchemaVersion = ParseInt(GetValue(values, "schema")),
                    Table = GetValue(values, "table").Trim(),
                    Key = GetValue(values, "key").Trim(),
                    Id = ParseLong(GetValue(values, "id")),
                    Source = GetValue(values, "source"),
                    Status = GetValue(values, "status").Trim(),
                    IsSmart = ParseBool(GetValue(values, "smart")),
                    AssetType = GetValue(values, "asset_type").Trim(),
                    Preload = ParseBool(GetValue(values, "preload")),
                    SharedComment = GetValue(values, "shared_comment"),
                    Tags = GetValue(values, "tags"),
                    Context = GetValue(values, "context"),
                    LocaleValues = localizedValues,
                    LocaleComments = localizedComments,
                });
            }

            return new LocalizationCsvDocument
            {
                Headers = headers,
                LocaleColumns = localeColumns,
                Rows = rows,
            };
        }

        private static bool IsLocaleColumn(string header)
        {
            return !string.IsNullOrWhiteSpace(header) &&
                   !ReservedColumns.Contains(header) &&
                   !header.EndsWith(
                       "_comment",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string GetValue(
            IReadOnlyDictionary<string, string> values,
            string key)
        {
            return values.TryGetValue(key, out var value)
                ? value
                : string.Empty;
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var result)
                ? result
                : 0;
        }

        private static long ParseLong(string value)
        {
            return long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var result)
                ? result
                : 0;
        }

        private static bool ParseBool(string value)
        {
            return string.Equals(
                       value,
                       "true",
                       StringComparison.OrdinalIgnoreCase) ||
                   value == "1";
        }

        private static List<CsvRecord> ReadRecords(string content)
        {
            var records = new List<CsvRecord>();
            var fields = new List<string>();
            var field = new StringBuilder();
            var quoted = false;
            var line = 1;
            var recordLine = 1;
            var column = 1;

            for (var index = 0; index < content.Length; index++)
            {
                var character = content[index];
                if (quoted)
                {
                    if (character == '"')
                    {
                        if (index + 1 < content.Length && content[index + 1] == '"')
                        {
                            field.Append('"');
                            index++;
                            column += 2;
                            continue;
                        }

                        quoted = false;
                    }
                    else
                    {
                        field.Append(character);
                    }

                    if (character == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }

                    continue;
                }

                switch (character)
                {
                    case '"' when field.Length == 0:
                        quoted = true;
                        column++;
                        break;
                    case ',':
                        fields.Add(field.ToString());
                        field.Clear();
                        column++;
                        break;
                    case '\r':
                        if (index + 1 < content.Length && content[index + 1] == '\n')
                        {
                            index++;
                        }

                        AddRecord(records, fields, field, recordLine);
                        line++;
                        recordLine = line;
                        column = 1;
                        break;
                    case '\n':
                        AddRecord(records, fields, field, recordLine);
                        line++;
                        recordLine = line;
                        column = 1;
                        break;
                    default:
                        field.Append(character);
                        column++;
                        break;
                }
            }

            if (quoted)
            {
                throw new LocalizationCsvParseException(
                    "CSV contains an unterminated quoted field.",
                    line,
                    column);
            }

            if (field.Length > 0 || fields.Count > 0)
            {
                AddRecord(records, fields, field, recordLine);
            }

            return records;
        }

        private static void AddRecord(
            ICollection<CsvRecord> records,
            ICollection<string> fields,
            StringBuilder field,
            int line)
        {
            fields.Add(field.ToString());
            field.Clear();
            records.Add(new CsvRecord(line, fields.ToList()));
            fields.Clear();
        }

        private sealed class CsvRecord
        {
            public CsvRecord(int line, IReadOnlyList<string> fields)
            {
                this.Line = line;
                this.Fields = fields;
            }

            public int Line { get; }

            public IReadOnlyList<string> Fields { get; }
        }
    }
}
