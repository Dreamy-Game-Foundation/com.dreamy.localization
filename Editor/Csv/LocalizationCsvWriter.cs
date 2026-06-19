using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Dreamy.Localization.Editor.Csv
{
    public static class LocalizationCsvWriter
    {
        private static readonly string[] StandardColumns =
        {
            "schema",
            "table",
            "key",
            "id",
            "source",
        };

        public static string Write(LocalizationCsvDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var locales = document.LocaleColumns
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
            var headers = BuildHeaders(locales);
            var builder = new StringBuilder();
            AppendRecord(builder, headers);

            foreach (var row in document.Rows
                         .OrderBy(value => value.Table, StringComparer.Ordinal)
                         .ThenBy(value => value.Key, StringComparer.Ordinal))
            {
                var fields = new List<string>
                {
                    row.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                    row.Table,
                    row.Key,
                    row.Id > 0
                        ? row.Id.ToString(CultureInfo.InvariantCulture)
                        : string.Empty,
                    row.Source,
                };

                foreach (var locale in locales)
                {
                    fields.Add(GetValue(row.LocaleValues, locale));
                }

                fields.Add(row.Status);
                fields.Add(row.IsSmart ? "true" : "false");
                fields.Add(row.AssetType);
                fields.Add(row.Preload ? "true" : "false");
                fields.Add(row.SharedComment);
                foreach (var locale in locales)
                {
                    fields.Add(GetValue(row.LocaleComments, locale));
                }

                fields.Add(row.Tags);
                fields.Add(row.Context);
                AppendRecord(builder, fields);
            }

            return builder.ToString();
        }

        private static IReadOnlyList<string> BuildHeaders(
            IReadOnlyList<string> locales)
        {
            var headers = new List<string>(StandardColumns);
            headers.AddRange(locales);
            headers.Add("status");
            headers.Add("smart");
            headers.Add("asset_type");
            headers.Add("preload");
            headers.Add("shared_comment");
            headers.AddRange(locales.Select(locale => $"{locale}_comment"));
            headers.Add("tags");
            headers.Add("context");
            return headers;
        }

        private static string GetValue(
            IReadOnlyDictionary<string, string> values,
            string key)
        {
            return values != null && values.TryGetValue(key, out var value)
                ? value
                : string.Empty;
        }

        private static void AppendRecord(
            StringBuilder builder,
            IEnumerable<string> fields)
        {
            builder.Append(string.Join(",", fields.Select(Escape)));
            builder.Append('\n');
        }

        private static string Escape(string value)
        {
            value ??= string.Empty;
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
