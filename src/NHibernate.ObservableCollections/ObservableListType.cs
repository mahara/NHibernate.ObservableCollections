using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.UserTypes;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     The NHibernate type for a generic list collection that fires events
    ///     when item(s) have been added to or removed from the collection.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of items in the list.
    /// </typeparam>
    /// <remarks>
    ///     AUTHORS:
    ///     -   Adrian Alexander
    ///     REFERENCES:
    ///     -   <see href="https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2008/05/revisiting-bidirectional-assoc-helpers.html" />
    /// </remarks>
    public class ObservableListType<T> : IUserCollectionType
    {
        /// <inheritdoc />
        public bool Contains(object collection, object entity)
        {
            return ((IList<T>) collection).Contains((T) entity);
        }

        /// <inheritdoc />
        public IEnumerable GetElements(object collection)
        {
            return (IEnumerable) collection;
        }

        /// <inheritdoc />
        public object IndexOf(object collection, object entity)
        {
            return ((IList<T>) collection).IndexOf((T) entity);
        }

        /// <inheritdoc />
        public object Instantiate(int anticipatedSize)
        {
            return new ObservableList<T>();
        }

        /// <inheritdoc />
        public IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister)
        {
            return new PersistentObservableList<T>(session);
        }

        /// <inheritdoc />
        public object ReplaceElements(object original, object target, ICollectionPersister persister, object owner, IDictionary copyCache, ISessionImplementor session)
        {
            var result = (IList<T>) target;

            result.Clear();

            foreach (var item in (IEnumerable) original)
            {
                result.Add((T) item);
            }

            return result;
        }

        /// <inheritdoc />
        public IPersistentCollection Wrap(ISessionImplementor session, object collection)
        {
            return new PersistentObservableList<T>(session, (ObservableList<T>) collection);
        }
    }
}
