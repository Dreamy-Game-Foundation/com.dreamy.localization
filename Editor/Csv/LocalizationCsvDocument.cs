using System.Collections.Generic;

namespace Dreamy.Localization.Editor.Csv
{
    public sealed class LocalizationCsvDocument
    {
        public IReadOnlyList<string> Headers { get; set; } = new List<string>();

        public IReadOnlyList<string> LocaleColumns { get; set; } = new List<string>();

        public IReadOnlyList<LocalizationCsvRow> Rows { get; set; } =
            new List<LocalizationCsvRow>();
    }
}
