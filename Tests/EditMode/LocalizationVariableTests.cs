using NUnit.Framework;

namespace Dreamy.Localization.Tests
{
    public sealed class LocalizationVariableTests
    {
        [Test]
        public void TypedValue_WhenChanged_RaisesOneNotification()
        {
            var variable = new LocalizationVariable<int>(1);
            var notifications = 0;
            variable.ValueChanged += () => notifications++;

            variable.TypedValue = 2;
            variable.TypedValue = 2;

            Assert.That(notifications, Is.EqualTo(1));
        }

        [Test]
        public void Registry_RegisterAndRead_ReturnsTypedValue()
        {
            var registry = new LocalizationVariableRegistry();
            registry.Register("coins", new LocalizationVariable<int>(42));

            var found = registry.TryGetValue("coins", out int value);

            Assert.That(found, Is.True);
            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void Registry_VariableChange_ReportsOnlyChangedName()
        {
            var registry = new LocalizationVariableRegistry();
            var coins = new LocalizationVariable<int>(1);
            registry.Register("coins", coins);
            registry.Register("gems", new LocalizationVariable<int>(2));
            string changedName = null;
            registry.VariableChanged += name => changedName = name;

            coins.TypedValue = 3;

            Assert.That(changedName, Is.EqualTo("coins"));
        }
    }
}
