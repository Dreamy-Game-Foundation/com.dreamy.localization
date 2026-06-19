using System.Linq;
using Dreamy.Localization.Editor.Csv;
using NUnit.Framework;

namespace Dreamy.Localization.Tests
{
    public sealed class LocalizationCsvParserTests
    {
        [Test]
        public void Parse_QuotedFields_PreservesCommasQuotesAndNewlines()
        {
            const string csv =
                "schema,table,key,source,en,vi,status,smart\n" +
                "1,UI,home.title,\"Hello, \"\"Player\"\"\"," +
                "\"Hello,\n{player}\",\"Xin chao, {player}\",approved,true\n";

            var document = LocalizationCsvParser.Parse(csv);
            var row = document.Rows.Single();

            Assert.That(row.Table, Is.EqualTo("UI"));
            Assert.That(row.Key, Is.EqualTo("home.title"));
            Assert.That(row.Source, Is.EqualTo("Hello, \"Player\""));
            Assert.That(row.LocaleValues["en"], Is.EqualTo("Hello,\n{player}"));
            Assert.That(row.IsSmart, Is.True);
        }

        [Test]
        public void Parse_Utf8Bom_DoesNotBecomePartOfFirstHeader()
        {
            const string csv =
                "\uFEFFschema,table,key,en\n" +
                "1,UI,title,Title\n";

            var document = LocalizationCsvParser.Parse(csv);

            Assert.That(document.Headers[0], Is.EqualTo("schema"));
        }

        [Test]
        public void Parse_UnterminatedQuote_ThrowsWithLocation()
        {
            const string csv = "schema,table,key,en\n1,UI,title,\"Title";

            var exception = Assert.Throws<LocalizationCsvParseException>(
                () => LocalizationCsvParser.Parse(csv));

            Assert.That(exception.Line, Is.EqualTo(2));
            Assert.That(exception.Column, Is.GreaterThan(1));
        }

        [Test]
        public void WriteThenParse_PreservesSemanticValues()
        {
            const string csv =
                "schema,table,key,id,source,en,vi,status,smart,shared_comment\n" +
                "1,UI,title,42,\"Hello, Player\",\"Hello, Player\",Xin chao,approved,false,Header\n";

            var first = LocalizationCsvParser.Parse(csv);
            var written = LocalizationCsvWriter.Write(first);
            var second = LocalizationCsvParser.Parse(written);

            Assert.That(second.Rows.Count, Is.EqualTo(1));
            Assert.That(second.Rows[0].Id, Is.EqualTo(42));
            Assert.That(second.Rows[0].Source, Is.EqualTo("Hello, Player"));
            Assert.That(second.Rows[0].LocaleValues["vi"], Is.EqualTo("Xin chao"));
            Assert.That(second.Rows[0].SharedComment, Is.EqualTo("Header"));
        }
    }
}
