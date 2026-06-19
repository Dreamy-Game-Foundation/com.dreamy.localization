namespace Dreamy.Localization.Editor.Validation
{
    public sealed class LocalizationValidationIssue
    {
        public LocalizationValidationSeverity Severity { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }

        public int Line { get; set; }

        public string Key { get; set; }

        public override string ToString()
        {
            return $"{this.Severity} [{this.Code}] line {this.Line}: {this.Message}";
        }
    }
}
