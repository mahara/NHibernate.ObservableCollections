using System.Diagnostics;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     VS IDE can't differentiate between types with the same name from different assembly.
    ///     So we need to use different names for collection debug view
    ///     for collections in mscorlib.dll and system.dll.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///     REFERENCES:
    ///     -   <see href="https://referencesource.microsoft.com/#mscorlib/system/collections/generic/debugview.cs" />
    /// </remarks>
    internal sealed class CollectionDebugView<T>
    {
        private readonly ICollection<T> _collection;

        public CollectionDebugView(ICollection<T> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
