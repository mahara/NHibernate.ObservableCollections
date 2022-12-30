namespace Iesi.Collections.Generic
{
    using System.Diagnostics;

    /// <summary>
    ///     VS IDE can't differentiate between types with the same name from different assembly.
    ///     So we need to use different names for collection debug view
    ///     for collections in mscorlib.dll and system.dll.
    ///     <see href="http://referencesource.microsoft.com/#mscorlib/system/collections/generic/debugview.cs" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Mscorlib_CollectionDebugView<T>
    {
        private readonly ICollection<T> _collection;

        public Mscorlib_CollectionDebugView(ICollection<T> collection)
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

    internal sealed class Mscorlib_DictionaryKeyCollectionDebugView<TKey, TValue>
    {

        private readonly ICollection<TKey> _collection;

        public Mscorlib_DictionaryKeyCollectionDebugView(ICollection<TKey> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items
        {
            get
            {
                var items = new TKey[_collection.Count];

                _collection.CopyTo(items, 0);

                return items;
            }
        }
    }

    internal sealed class Mscorlib_DictionaryValueCollectionDebugView<TKey, TValue>
    {
        private readonly ICollection<TValue> _collection;

        public Mscorlib_DictionaryValueCollectionDebugView(ICollection<TValue> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                var items = new TValue[_collection.Count];

                _collection.CopyTo(items, 0);

                return items;
            }
        }
    }

    internal sealed class Mscorlib_DictionaryDebugView<K, V>
    {
        private readonly IDictionary<K, V> _dictionary;

        public Mscorlib_DictionaryDebugView(IDictionary<K, V> dictionary)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items
        {
            get
            {
                var items = new KeyValuePair<K, V>[_dictionary.Count];

                _dictionary.CopyTo(items, 0);

                return items;
            }
        }
    }

    internal sealed class Mscorlib_KeyedCollectionDebugView<TKey, TValue> where TKey : notnull
    {
        private readonly KeyedCollection<TKey, TValue> _keyedCollection;

        public Mscorlib_KeyedCollectionDebugView(KeyedCollection<TKey, TValue> keyedCollection)
        {
            _keyedCollection = keyedCollection ?? throw new ArgumentNullException(nameof(keyedCollection));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                var items = new TValue[_keyedCollection.Count];

                _keyedCollection.CopyTo(items, 0);

                return items;
            }
        }
    }
}
