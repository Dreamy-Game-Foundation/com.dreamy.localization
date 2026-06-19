using System;

namespace Dreamy.Localization.Editor.Csv
{
    public sealed class LocalizationCsvParseException : Exception
    {
        public LocalizationCsvParseException(string message, int line, int column)
            : base($"{message} Line {line}, column {column}.")
        {
            this.Line = line;
            this.Column = column;
        }

        public int Line { get; }

        public int Column { get; }
    }
}
