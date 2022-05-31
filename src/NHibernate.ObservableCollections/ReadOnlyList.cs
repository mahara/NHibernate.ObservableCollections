namespace Iesi.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    [Serializable]
    [ComVisible(false)]
    [DebuggerTypeProxy(typeof(Mscorlib_CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ReadOnlyList<T> : IList<T>, IList, IReadOnlyList<T>
    {
        [NonSerialized]
        private object _syncRoot;

        public ReadOnlyList(IList<T> list)
        {
            if (list == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list);
            }

            InnerList = list;
        }

        protected IList<T> InnerList { get; }

        bool ICollection.IsSynchronized =>
            false;

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    if (InnerList is ICollection c)
                    {
                        _syncRoot = c.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                    }
                }

                return _syncRoot;
            }
        }

        bool IList.IsReadOnly =>
            true;

        bool IList.IsFixedSize =>
            true;

        object IList.this[int index]
        {
            get => InnerList[index];
            set => ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection) InnerList).CopyTo(array, index);
        }

        int IList.Add(object value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);

            return -1;
        }

        void IList.Insert(int index, object value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void IList.Remove(object value)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void IList.RemoveAt(int index)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void IList.Clear()
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T) value);
            }

            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return IndexOf((T) value);
            }

            return -1;
        }

        public int Count =>
            InnerList.Count;

        bool ICollection<T>.IsReadOnly =>
            true;

        T IList<T>.this[int index]
        {
            get => InnerList[index];
            set => ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        public bool Contains(T item)
        {
            return InnerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InnerList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return InnerList.IndexOf(item);
        }

        void ICollection<T>.Add(T item)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        bool ICollection<T>.Remove(T item)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);

            return false;
        }

        void ICollection<T>.Clear()
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) InnerList).GetEnumerator();
        }

        void IList<T>.Insert(int index, T item)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        void IList<T>.RemoveAt(int index)
        {
            ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
        }

        public T this[int index] =>
            InnerList[index];

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>.
            return value is T || value == null && default(T) == null;
        }
    }
}
