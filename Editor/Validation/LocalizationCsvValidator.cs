using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dreamy.Localization.Editor.Csv;

namespace Dreamy.Localization.Editor.Validation
{
    public static class LocalizationCsvValidator
    {
        public const int CurrentSchemaVersion = 1;

        private static readonly HashSet<string> AllowedStatuses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                string.Empty,
                "draft",
                "translated",
                "review",
                "approved",
                "deprecated",
                "ignore",
            };

        private static readonly Regex PlaceholderPattern =
            new(@"\{(?<name>[A-Za-z_][A-Za-z0-9_.]*)", RegexOptions.Compiled);

        public static IReadOnlyList<LocalizationValidationIssue> Validate(
            LocalizationCsvDocument document)
        {
            var issues = new List<LocalizationValidationIssue>();
            if (document == null)
            {
                issues.Add(Create(
                    LocalizationValidationSeverity.Critical,
                    "csv.null_document",
                    "CSV document is null."));
                return issues;
            }

            ValidateHeaders(document, issues);
            ValidateRows(document, issues);
            return issues;
        }

        private static void ValidateHeaders(
            LocalizationCsvDocument document,
            ICollection<LocalizationValidationIssue> issues)
        {
            var duplicates = document.Headers
                .Where(header => !string.IsNullOrWhiteSpace(header))
                .GroupBy(header => header, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1);
            foreach (var duplicate in duplicates)
            {
                issues.Add(Create(
                    LocalizationValidationSeverity.Critical,
                    "csv.duplicate_header",
                    $"Header '{duplicate.Key}' appears more than once."));
            }

            foreach (var required in new[] { "schema", "table", "key" })
            {
                if (!document.Headers.Contains(
                        required,
                        StringComparer.OrdinalIgnoreCase))
                {
                    issues.Add(Create(
                        LocalizationValidationSeverity.Critical,
                        "csv.missing_header",
                        $"Required header '{required}' is missing."));
                }
            }

            if (document.LocaleColumns.Count == 0)
            {
                issues.Add(Create(
                    LocalizationValidationSeverity.Critical,
                    "csv.missing_locale",
                    "At least one locale column is required."));
            }
        }

        private static void ValidateRows(
            LocalizationCsvDocument document,
            ICollection<LocalizationValidationIssue> issues)
        {
            var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in document.Rows)
            {
                if (row.SchemaVersion != CurrentSchemaVersion)
                {
                    issues.Add(CreateRow(
                        row,
                        LocalizationValidationSeverity.Critical,
                        "csv.unsupported_schema",
                        $"Schema version '{row.SchemaVersion}' is not supported."));
                }

                if (string.IsNullOrWhiteSpace(row.Table))
                {
                    issues.Add(CreateRow(
                        row,
                        LocalizationValidationSeverity.Error,
                        "csv.missing_table",
                        "Table is required."));
                }

                if (string.IsNullOrWhiteSpace(row.Key))
                {
                    issues.Add(CreateRow(
                        row,
                        LocalizationValidationSeverity.Error,
                        "csv.missing_key",
                        "Key is required."));
                }

                if (!string.IsNullOrWhiteSpace(row.Table) &&
                    !string.IsNullOrWhiteSpace(row.Key) &&
                    !identities.Add(row.Identity))
                {
                    issues.Add(CreateRow(
                        row,
                        LocalizationValidationSeverity.Critical,
                        "csv.duplicate_key",
                        $"Duplicate table/key '{row.Identity}'."));
                }

                if (!AllowedStatuses.Contains(row.Status))
                {
                    issues.Add(CreateRow(
                        row,
                        LocalizationValidationSeverity.Error,
                        "csv.invalid_status",
                        $"Status '{row.Status}' is invalid."));
                }

                if (string.Equals(
                        row.Status,
                        "ignore",
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        row.Status,
                        "deprecated",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var sourcePlaceholders = ExtractPlaceholders(row.Source);
                foreach (var locale in document.LocaleColumns)
                {
                    var value = GetValue(row.LocaleValues, locale);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        issues.Add(CreateRow(
                            row,
                            LocalizationValidationSeverity.Error,
                            "csv.missing_translation",
                            $"Translation for locale '{locale}' is missing."));
                        continue;
                    }

                    if (!row.IsSmart || string.IsNullOrEmpty(row.Source))
                    {
                        continue;
                    }

                    var localizedPlaceholders = ExtractPlaceholders(value);
                    if (!sourcePlaceholders.SetEquals(localizedPlaceholders))
                    {
                        issues.Add(CreateRow(
                            row,
                            LocalizationValidationSeverity.Error,
                            "csv.placeholder_mismatch",
                            $"Smart String placeholders for locale '{locale}' do not match source."));
                    }
                }
            }
        }

        private static HashSet<string> ExtractPlaceholders(string value)
        {
            var placeholders = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(value))
            {
                return placeholders;
            }

            foreach (Match match in PlaceholderPattern.Matches(value))
            {
                placeholders.Add(match.Groups["name"].Value);
            }

            return placeholders;
        }

        private static string GetValue(
            IReadOnlyDictionary<string, string> values,
            string locale)
        {
            return values != null && values.TryGetValue(locale, out var value)
                ? value
                : string.Empty;
        }

        private static LocalizationValidationIssue CreateRow(
            LocalizationCsvRow row,
            LocalizationValidationSeverity severity,
            string code,
            string message)
        {
            return new LocalizationValidationIssue
            {
                Severity = severity,
                Code = code,
                Message = message,
                Line = row.SourceLine,
                Key = row.Identity,
            };
        }

        private static LocalizationValidationIssue Create(
            LocalizationValidationSeverity severity,
            string code,
            string message)
        {
            return new LocalizationValidationIssue
            {
                Severity = severity,
                Code = code,
                Message = message,
            };
        }
    }
}
