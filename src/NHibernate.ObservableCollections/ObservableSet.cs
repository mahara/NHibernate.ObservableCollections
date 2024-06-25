using System.Diagnostics;

namespace Iesi.Collections.Generic;

/// <summary>
///     A generic set that fires events when item(s) have been added to or removed from the set.
/// </summary>
/// <typeparam name="T">
///     The type of items in the set.
/// </typeparam>
/// <remarks>
///     REFERENCES:
///     -   <see href="https://github.com/dotnet/efcore/blob/main/src/EFCore/ChangeTracking/ObservableHashSet.cs" />
///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs" />
/// </remarks>
[Serializable]
[DebuggerTypeProxy(typeof(CollectionDebugView<>))]
[DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
public class ObservableSet<T> :
    ISet<T>, IReadOnlyCollection<T>,
    INotifyCollectionChanged, INotifyPropertyChanging, INotifyPropertyChanged
{
    private HashSet<T> _set;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableSet{T}" /> class
    ///     that is empty and uses the default equality comparer for the set type.
    /// </summary>
    public ObservableSet() :
        this(EqualityComparer<T>.Default)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableSet{T}" /> class
    ///     that is empty and uses the specified equality comparer for the set type.
    /// </summary>
    /// <param name="comparer">
    ///     The <see cref="IEqualityComparer{T}" /> implementation to use when
    ///     comparing values in the set, or null to use the default <see cref="IEqualityComparer{T}" />
    ///     implementation for the set type.
    /// </param>
    public ObservableSet(IEqualityComparer<T> comparer)
    {
        _set = new HashSet<T>(comparer);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableSet{T}" /> class
    ///     that uses the default equality comparer for the set type, contains elements copied
    ///     from the specified collection, and has sufficient capacity to accommodate the
    ///     number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    public ObservableSet(IEnumerable<T> collection) :
        this(collection, EqualityComparer<T>.Default)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableSet{T}" /> class
    ///     that uses the specified equality comparer for the set type, contains elements
    ///     copied from the specified collection, and has sufficient capacity to accommodate
    ///     the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="comparer">
    ///     The <see cref="IEqualityComparer{T}" /> implementation to use when comparing values in the set,
    ///     or null to use the default <see cref="IEqualityComparer{T}" /> implementation for the set type.
    /// </param>
    public ObservableSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
        _set = new HashSet<T>(collection, comparer);
    }

    /// <summary>
    ///     Occurs when the contents of the hash set changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///     Occurs when a property of this hash set (such as <see cref="Count" />) is changing.
    /// </summary>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    ///     Occurs when a property of this hash set (such as <see cref="Count" />) changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets the <see cref="IEqualityComparer{T}" /> object that is used to determine equality for the values in the set.
    /// </summary>
    public virtual IEqualityComparer<T> Comparer =>
        _set.Comparer;

    /// <summary>
    ///     Gets the number of elements that are contained in the hash set.
    /// </summary>
    public virtual int Count =>
        _set.Count;

    /// <summary>
    ///     Gets a value indicating whether the hash set is read-only.
    /// </summary>
    public virtual bool IsReadOnly =>
        ((ICollection<T>) _set).IsReadOnly;

    /// <summary>
    ///     Determines whether the hash set object contains the specified element.
    /// </summary>
    /// <param name="item">The element to locate in the hash set.</param>
    /// <returns>
    ///     <see langword="true" /> if the hash set contains the specified element; otherwise, <see langword="false" />.
    /// </returns>
    public virtual bool Contains(T item)
    {
        return _set.Contains(item);
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    /// <summary>
    ///     Adds the specified element to the hash set.
    /// </summary>
    /// <param name="item">The element to add to the set.</param>
    /// <returns>
    ///     <see langword="true" /> if the element is added to the hash set; <see langword="false" /> if the element is already present.
    /// </returns>
    public virtual bool Add(T item)
    {
        if (_set.Contains(item))
        {
            return false;
        }

        OnCountPropertyChanging();

        _set.Add(item);

        OnCountPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item);

        return true;
    }

    /// <summary>
    ///     Removes the specified element from the hash set.
    /// </summary>
    /// <param name="item">The element to remove.</param>
    /// <returns>
    ///     <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.
    /// </returns>
    public virtual bool Remove(T item)
    {
        if (!_set.Contains(item))
        {
            return false;
        }

        OnCountPropertyChanging();

        _set.Remove(item);

        OnCountPropertyChanged();
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item);

        return true;
    }

    /// <summary>
    ///     Removes all elements that match the conditions defined by the specified predicate
    ///     from the hash set.
    /// </summary>
    /// <param name="match">
    ///     The <see cref="Predicate{T}" /> delegate that defines the conditions of the elements to remove.
    /// </param>
    /// <returns>The number of elements that were removed from the hash set.</returns>
    public virtual int RemoveWhere(Predicate<T> match)
    {
        var copy = new HashSet<T>(_set, _set.Comparer);

        var removedCount = copy.RemoveWhere(match);

        if (removedCount == 0)
        {
            return 0;
        }

        var removed = _set.Where(i => !copy.Contains(i)).ToList();

        OnCountPropertyChanging();

        _set = copy;

        OnCountPropertyChanged();
        OnCollectionChanged(removed, EventArgsCache.ItemsEmpty);

        return removedCount;
    }

    /// <summary>
    ///     Sets the capacity of the hash set to the actual number of elements it contains, rounded up to a nearby,
    ///     implementation-specific value.
    /// </summary>
    public virtual void TrimExcess()
    {
        _set.TrimExcess();
    }

    /// <summary>
    ///     Removes all elements from the hash set.
    /// </summary>
    public virtual void Clear()
    {
        if (_set.Count == 0)
        {
            return;
        }

        OnCountPropertyChanging();

        _set.Clear();

        OnCountPropertyChanged();
        OnCollectionReset();
    }

    /// <summary>
    ///     Modifies the hash set to contain all elements that are present in itself, the specified collection, or both.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    public virtual void UnionWith(IEnumerable<T> other)
    {
        var copy = new HashSet<T>(_set, _set.Comparer);

        copy.UnionWith(other);

        if (copy.Count == _set.Count)
        {
            return;
        }

        var added = copy.Where(i => !_set.Contains(i)).ToList();

        OnCountPropertyChanging();

        _set = copy;

        OnCountPropertyChanged();
        OnCollectionChanged(EventArgsCache.ItemsEmpty, added);
    }

    /// <summary>
    ///     Modifies the current hash set to contain only
    ///     elements that are present in that object and in the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    public virtual void IntersectWith(IEnumerable<T> other)
    {
        var copy = new HashSet<T>(_set, _set.Comparer);

        copy.IntersectWith(other);

        if (copy.Count == _set.Count)
        {
            return;
        }

        var removed = _set.Where(i => !copy.Contains(i)).ToList();

        OnCountPropertyChanging();

        _set = copy;

        OnCountPropertyChanged();
        OnCollectionChanged(removed, EventArgsCache.ItemsEmpty);
    }

    /// <summary>
    ///     Removes all elements in the specified collection from the hash set.
    /// </summary>
    /// <param name="other">The collection of items to remove from the current hash set.</param>
    public virtual void ExceptWith(IEnumerable<T> other)
    {
        var copy = new HashSet<T>(_set, _set.Comparer);

        copy.ExceptWith(other);

        if (copy.Count == _set.Count)
        {
            return;
        }

        var removed = _set.Where(i => !copy.Contains(i)).ToList();

        OnCountPropertyChanging();

        _set = copy;

        OnCountPropertyChanged();
        OnCollectionChanged(removed, EventArgsCache.ItemsEmpty);
    }

    /// <summary>
    ///     Modifies the current hash set to contain only elements that are present either in that
    ///     object or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    public virtual void SymmetricExceptWith(IEnumerable<T> other)
    {
        var copy = new HashSet<T>(_set, _set.Comparer);

        copy.SymmetricExceptWith(other);

        var removed = _set.Where(i => !copy.Contains(i)).ToList();
        var added = copy.Where(i => !_set.Contains(i)).ToList();

        if (removed.Count == 0 && added.Count == 0)
        {
            return;
        }

        OnCountPropertyChanging();

        _set = copy;

        OnCountPropertyChanged();
        OnCollectionChanged(removed, added);
    }

    /// <summary>
    ///     Determines whether the hash set is a subset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    /// <returns>
    ///     <see langword="true" /> if the hash set is a subset of other; otherwise, <see langword="false" />.
    /// </returns>
    public virtual bool IsSubsetOf(IEnumerable<T> other)
    {
        return _set.IsSubsetOf(other);
    }

    /// <summary>
    ///     Determines whether the hash set is a proper subset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    /// <returns>
    ///     <see langword="true" /> if the hash set is a proper subset of other; otherwise, <see langword="false" />.
    /// </returns>
    public virtual bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return _set.IsProperSubsetOf(other);
    }

    /// <summary>
    ///     Determines whether the hash set is a superset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    /// <returns>
    ///     <see langword="true" /> if the hash set is a superset of other; otherwise, <see langword="false" />.
    /// </returns>
    public virtual bool IsSupersetOf(IEnumerable<T> other)
    {
        return _set.IsSupersetOf(other);
    }

    /// <summary>
    ///     Determines whether the hash set is a proper superset of the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    /// <returns>
    ///     <see langword="true" /> if the hash set is a proper superset of other; otherwise, <see langword="false" />.
    /// </returns>
    public virtual bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return _set.IsProperSupersetOf(other);
    }

    /// <summary>
    ///     Determines whether the current System.Collections.Generic.HashSet`1 object and a specified collection share common elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    /// <returns>
    ///     <see langword="true" /> if the hash set and other share at least one common element; otherwise, <see langword="false" />.
    /// </returns>
    public virtual bool Overlaps(IEnumerable<T> other)
    {
        return _set.Overlaps(other);
    }

    /// <summary>
    ///     Determines whether the hash set and the specified collection contain the same elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current hash set.</param>
    /// <returns>
    ///     <see langword="true" /> if the hash set is equal to other; otherwise, <see langword="false" />.
    /// </returns>
    public virtual bool SetEquals(IEnumerable<T> other)
    {
        return _set.SetEquals(other);
    }

    /// <summary>
    ///     Copies the elements of the hash set to an array.
    /// </summary>
    /// <param name="array">
    ///     The one-dimensional array that is the destination of the elements copied from
    ///     the hash set. The array must have zero-based indexing.
    /// </param>
    public virtual void CopyTo(T[] array)
    {
        _set.CopyTo(array);
    }

    /// <summary>
    ///     Copies the elements of the hash set to an array, starting at the specified array index.
    /// </summary>
    /// <param name="array">
    ///     The one-dimensional array that is the destination of the elements copied from
    ///     the hash set. The array must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public virtual void CopyTo(T[] array, int arrayIndex)
    {
        _set.CopyTo(array, arrayIndex);
    }

    /// <summary>
    ///     Copies the specified number of elements of the hash set to an array, starting at the specified array index.
    /// </summary>
    /// <param name="array">
    ///     The one-dimensional array that is the destination of the elements copied from
    ///     the hash set. The array must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    /// <param name="count">The number of elements to copy to array.</param>
    public virtual void CopyTo(T[] array, int arrayIndex, int count)
    {
        _set.CopyTo(array, arrayIndex, count);
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the hash set.
    /// </summary>
    /// <returns>
    ///     An enumerator for the hash set.
    /// </returns>
    public virtual HashSet<T>.Enumerator GetEnumerator()
    {
        return _set.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Helper to raise a ranged <see cref="CollectionChanged" /> event with action == Reset to any listeners.
    /// </summary>
    protected void OnCollectionReset()
    {
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
    }

    private void OnCollectionChanged(IList oldItems, IList newItems)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems));
    }

    /// <summary>
    ///     Raises the <see cref="CollectionChanged" /> event.
    /// </summary>
    /// <param name="e">Details of the change.</param>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    /// <summary>
    ///     Raises the <see cref="PropertyChanging" /> event.
    /// </summary>
    /// <param name="e">Details of the property that is changing.</param>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        PropertyChanging?.Invoke(this, e);
    }

    private void OnCountPropertyChanging()
    {
        OnPropertyChanging(EventArgsCache.CountPropertyChanging);
    }

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="e">Details of the property that changed.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    private void OnCountPropertyChanged()
    {
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    internal static class EventArgsCache
    {
        public static readonly T[] ItemsEmpty = [];

        public static readonly PropertyChangingEventArgs CountPropertyChanging = new(nameof(Count));
        public static readonly PropertyChangedEventArgs CountPropertyChanged = new(nameof(Count));
        public static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
    }
}
