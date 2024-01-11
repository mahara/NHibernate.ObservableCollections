namespace NHibernate.ObservableCollections.Helpers
{
    using System.Reflection;

    public static class ReflectionUtil
    {
        private static MethodInfo? isInitializedMethod;

        public static bool IsInitialized<T>(ICollection<T> collection)
        {
            if (isInitializedMethod is null)
            {
                var type = System.Type.GetType("NHibernate.NHibernateUtil, NHibernate");
                if (type is not null)
                {
                    isInitializedMethod = type.GetMethod("IsInitialized", BindingFlags.Static | BindingFlags.Public);
                }
            }

            if (isInitializedMethod is not null)
            {
                // true if the NHibernate assembly is present
                return (bool) isInitializedMethod.Invoke(
                    null,
                    new object[]
                    {
                        collection
                    })!;
            }

            return true;
        }

        public static ICollection<T> NavigateToManySide<T>(object start, string propertyName)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var property = start.GetType().GetProperty(propertyName, flags)!
                                .DeclaringType!.GetProperty(propertyName, flags)!;
            return (ICollection<T>) property.GetValue(start, null)!;
        }

        public static object NavigateToOneSide(object start, string propertyName)
        {
            var property = start.GetType().GetProperty(propertyName)!;
            return property.GetValue(start, null)!;
        }
    }
}
