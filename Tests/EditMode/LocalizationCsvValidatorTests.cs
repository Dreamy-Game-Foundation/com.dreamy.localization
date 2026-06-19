using System.Linq;
using Dreamy.Localization.Editor.Csv;
using Dreamy.Localization.Editor.Validation;
using NUnit.Framework;

namespace Dreamy.Localization.Tests
{
    public sealed class LocalizationCsvValidatorTests
    {
        [Test]
        public void Validate_DuplicateKey_ReturnsCriticalIssue()
        {
            const string csv =
                "schema,table,key,en,status\n" +
                "1,UI,title,Title,approved\n" +
                "1,UI,title,Title,approved\n";

            var issues = LocalizationCsvValidator.Validate(
                LocalizationCsvParser.Parse(csv));

            Assert.That(
                issues.Any(issue =>
                    issue.Code == "csv.duplicate_key" &&
                    issue.Severity == LocalizationValidationSeverity.Critical),
                Is.True);
        }

        [Test]
        public void Validate_MissingTranslation_ReturnsError()
        {
            const string csv =
                "schema,table,key,en,vi,status\n" +
                "1,UI,title,Title,,approved\n";

            var issues = LocalizationCsvValidator.Validate(
                LocalizationCsvParser.Parse(csv));

            Assert.That(
                issues.Any(issue =>
                    issue.Code == "csv.missing_translation" &&
                    issue.Message.Contains("'vi'")),
                Is.True);
        }

        [Test]
        public void Validate_SmartPlaceholderMismatch_ReturnsError()
        {
            const string csv =
                "schema,table,key,source,en,vi,status,smart\n" +
                "1,UI,greeting,\"Hello {player}\",\"Hello {player}\",Xin chao,approved,true\n";

            var issues = LocalizationCsvValidator.Validate(
                LocalizationCsvParser.Parse(csv));

            Assert.That(
                issues.Any(issue => issue.Code == "csv.placeholder_mismatch"),
                Is.True);
        }

        [Test]
        public void Validate_IgnoreRow_AllowsEmptyTranslations()
        {
            const string csv =
                "schema,table,key,en,vi,status\n" +
                "1,UI,debug.label,,,ignore\n";

            var issues = LocalizationCsvValidator.Validate(
                LocalizationCsvParser.Parse(csv));

            Assert.That(
                issues.Any(issue => issue.Code == "csv.missing_translation"),
                Is.False);
        }
    }
}
