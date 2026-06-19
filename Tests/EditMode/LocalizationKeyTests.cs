using NUnit.Framework;

namespace Dreamy.Localization.Tests
{
    public sealed class LocalizationKeyTests
    {
        [Test]
        public void Equality_SameTableAndEntry_AreEqual()
        {
            var left = new LocalizationKey("UI", "home.title");
            var right = new LocalizationKey("UI", "home.title");

            Assert.That(left, Is.EqualTo(right));
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void IsValid_MissingEntry_IsFalse()
        {
            var key = new LocalizationKey("UI", string.Empty);

            Assert.That(key.IsValid, Is.False);
        }

        [Test]
        public void LocaleMatcher_CultureVariant_FallsBackToLanguageCode()
        {
            var result = LocaleCodeMatcher.Match(
                "en-US",
                new[] { "en", "vi" },
                null);

            Assert.That(result, Is.EqualTo("en"));
        }

        [Test]
        public void LocaleMatcher_ExactMatch_IsCaseInsensitive()
        {
            var result = LocaleCodeMatcher.Match(
                "VI",
                new[] { "en", "vi" },
                null);

            Assert.That(result, Is.EqualTo("vi"));
        }
    }
}
