using System.ComponentModel;

namespace Iesi.Collections.Generic.Tests
{
    public enum EventNotificationOrderMode
    {
        Strict,
        Relaxed
    }

    public abstract class TestBase
    {
        protected const string CountPropertyName = "Count";
        protected const string IndexerPropertyName = "Item[]";

        protected TestBase() :
            this(EventNotificationOrderMode.Strict)
        {
        }

        protected TestBase(EventNotificationOrderMode eventNotificationOrderMode)
        {
            EventNotificationOrderMode = eventNotificationOrderMode;
        }

        protected static string[] CountPropertyChangingEventPropertyNames =>
        [
            CountPropertyName,
        ];

        protected static string[] CountPropertyChangedEventPropertyNames =>
        [
            CountPropertyName,
        ];

        protected static string[] IndexerPropertyChangedEventPropertyNames =>
        [
            IndexerPropertyName,
        ];

        protected static string[] CountAndIndexerPropertyChangedEventPropertyNames =>
        [
            CountPropertyName,
            IndexerPropertyName,
        ];

        protected EventNotificationOrderMode EventNotificationOrderMode { get; }

        protected void AssertPropertyChangingEvents(
            IReadOnlyList<PropertyChangingEventArgs> propertyChangingEvents,
            string[] expectedPropertyNames)
        {
            AssertPropertyEventNames(
                [.. propertyChangingEvents.Select(static e => e.PropertyName!)],
                expectedPropertyNames);
        }

        protected void AssertPropertyChangedEvents(
            IReadOnlyList<PropertyChangedEventArgs> propertyChangedEvents,
            string[] expectedPropertyNames)
        {
            AssertPropertyEventNames(
                [.. propertyChangedEvents.Select(static e => e.PropertyName!)],
                expectedPropertyNames);
        }

        protected void AssertPropertyEventNames(
            string[] actualPropertyNames,
            string[] expectedPropertyNames)
        {
            Assert.That(actualPropertyNames, Has.Length.EqualTo(expectedPropertyNames.Length));

            if (EventNotificationOrderMode == EventNotificationOrderMode.Strict)
            {
                Assert.That(actualPropertyNames, Is.EqualTo(expectedPropertyNames));
            }
            else
            {
                Assert.That(actualPropertyNames, Is.EquivalentTo(expectedPropertyNames));
            }
        }

        protected void AssertPropertyChangingEvents(
            IReadOnlyList<PropertyChangingEventArgs> propertyChangingEvents,
            int expectedPropertyEventPatternCount,
            string[] expectedPropertyNames)
        {
            AssertPropertyEventNames(
                [.. propertyChangingEvents.Select(static e => e.PropertyName!)],
                RepeatPropertyEventPropertyNames(expectedPropertyEventPatternCount, expectedPropertyNames));
        }

        protected void AssertPropertyChangedEvents(
            IReadOnlyList<PropertyChangedEventArgs> propertyChangedEvents,
            int expectedPropertyEventPatternCount,
            string[] expectedPropertyNames)
        {
            AssertPropertyEventNames(
                [.. propertyChangedEvents.Select(static e => e.PropertyName!)],
                RepeatPropertyEventPropertyNames(expectedPropertyEventPatternCount, expectedPropertyNames));
        }

        private static string[] RepeatPropertyEventPropertyNames(
            int repeatCount,
            string[] propertyNames)
        {
            if (repeatCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(repeatCount), $"'{nameof(repeatCount)}' ({repeatCount}) cannot be less than zero.");
            }

            if (repeatCount == 0 || propertyNames.Length == 0)
            {
                return [];
            }

            var propertyNamesRepeated = new string[repeatCount * propertyNames.Length];

            for (var i = 0; i < repeatCount; i++)
            {
                Array.Copy(
                    propertyNames,
                    0,
                    propertyNamesRepeated,
                    i * propertyNames.Length,
                    propertyNames.Length);
            }

            return propertyNamesRepeated;
        }
    }
}
