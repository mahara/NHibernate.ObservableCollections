namespace Iesi.Collections.Generic
{
    using System.Diagnostics;

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///     REFERENCES:
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/Generic/DebugView.cs
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
