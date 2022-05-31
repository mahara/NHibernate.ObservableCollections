namespace Iesi.Collections.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    using static ThrowHelper;

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
            if (collection == null)
            {
                ThrowArgumentNullException(ExceptionArgument.collection);
            }

            _collection = collection;
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
            if (collection == null)
            {
                ThrowArgumentNullException(ExceptionArgument.collection);
            }

            _collection = collection;
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
            if (collection == null)
            {
                ThrowArgumentNullException(ExceptionArgument.collection);
            }

            _collection = collection;
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
        private readonly IDictionary<K, V> _dict;

        public Mscorlib_DictionaryDebugView(IDictionary<K, V> dictionary)
        {
            if (dictionary == null)
            {
                ThrowArgumentNullException(ExceptionArgument.dictionary);
            }

            _dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items
        {
            get
            {
                var items = new KeyValuePair<K, V>[_dict.Count];

                _dict.CopyTo(items, 0);

                return items;
            }
        }
    }

    internal sealed class Mscorlib_KeyedCollectionDebugView<K, T>
    {
        private readonly KeyedCollection<K, T> _kc;

        public Mscorlib_KeyedCollectionDebugView(KeyedCollection<K, T> keyedCollection)
        {
            _kc = keyedCollection ?? throw new ArgumentNullException(nameof(keyedCollection));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[_kc.Count];

                _kc.CopyTo(items, 0);

                return items;
            }
        }
    }
}