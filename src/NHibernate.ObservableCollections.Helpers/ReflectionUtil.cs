namespace NHibernate.ObservableCollections.Helpers
{
    using System.Reflection;

    public static class ReflectionUtil
    {
        private static MethodInfo? isInitialized;

        public static bool IsInitialized<T>(ICollection<T> newCollection)
        {
            if (isInitialized == null)
            {
                var t = System.Type.GetType("NHibernate.NHibernateUtil, NHibernate");
                if (t != null)
                {
                    isInitialized = t.GetMethod("IsInitialized", BindingFlags.Static | BindingFlags.Public);
                }
            }

            if (isInitialized != null)
            {
                // true if the NHibernate assembly is present
                return (bool) isInitialized.Invoke(
                    null,
                    new object[]
                    {
                        newCollection
                    })!;
            }

            return true;
        }

        public static ICollection<T> NavigateToManySide<T>(object start, string propertyName)
        {
            var bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var pi = start.GetType().GetProperty(propertyName, bf)!
                          .DeclaringType!.GetProperty(propertyName, bf)!;
            return (ICollection<T>) pi.GetValue(start, null)!;
        }

        public static object NavigateToOneSide(object start, string propertyName)
        {
            var pi = start.GetType().GetProperty(propertyName)!;
            return pi.GetValue(start, null)!;
        }
    }
}
