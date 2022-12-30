using System.Reflection;

namespace NHibernate.ObservableCollections.Helpers
{
    public static class ReflectionUtil
    {
        private static MethodInfo? _isInitializedMethod;

        public static bool IsInitialized<T>(ICollection<T> collection)
        {
            if (_isInitializedMethod is null)
            {
                var type = System.Type.GetType("NHibernate.NHibernateUtil, NHibernate");
                if (type is not null)
                {
                    _isInitializedMethod = type.GetMethod("IsInitialized", BindingFlags.Static | BindingFlags.Public);
                }
            }

            if (_isInitializedMethod is not null)
            {
                // true if the NHibernate assembly is present
                return (bool) _isInitializedMethod.Invoke(
                    null,
                    new object?[]
                    {
                        collection
                    })!;
            }

            return true;
        }

        public static ICollection<T> NavigateToManySide<T>(object start, string propertyName)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var property = start.GetType().GetProperty(propertyName, bindingFlags)!
                                .DeclaringType!.GetProperty(propertyName, bindingFlags)!;
            return (ICollection<T>) property.GetValue(start, null)!;
        }

        public static object NavigateToOneSide(object start, string propertyName)
        {
            var property = start.GetType().GetProperty(propertyName)!;
            return property.GetValue(start, null)!;
        }
    }
}
