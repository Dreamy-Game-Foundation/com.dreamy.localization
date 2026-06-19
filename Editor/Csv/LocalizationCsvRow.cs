using System;
using System.Collections.Generic;

namespace Dreamy.Localization.Editor.Csv
{
    public sealed class LocalizationCsvRow
    {
        public int SourceLine { get; set; }

        public int SchemaVersion { get; set; }

        public string Table { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public long Id { get; set; }

        public string Source { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool IsSmart { get; set; }

        public string AssetType { get; set; } = string.Empty;

        public bool Preload { get; set; }

        public string SharedComment { get; set; } = string.Empty;

        public string Tags { get; set; } = string.Empty;

        public string Context { get; set; } = string.Empty;

        public IReadOnlyDictionary<string, string> LocaleValues { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, string> LocaleComments { get; set; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string Identity => $"{this.Table}/{this.Key}";
    }
}
