using System.Diagnostics;

using NHibernate.DebugHelpers;
using NHibernate.Engine;
using NHibernate.Persister.Collection;

namespace Iesi.Collections.Generic;

/// <summary>
///     Represents a persistent observable collection.
/// </summary>
/// <typeparam name="T">
///     The type of the items in the collection.
/// </typeparam>
[Serializable]
[DebuggerTypeProxy(typeof(CollectionProxy<>))]
public class PersistentObservableCollection<T> : PersistentGenericList<T>, INotifyCollectionChanged
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PersistentObservableCollection{T}" /> class.
    /// </summary>
    /// <param name="session">
    ///     The session.
    /// </param>
    public PersistentObservableCollection(ISessionImplementor session) :
        base(session)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PersistentObservableCollection{T}" /> class.
    /// </summary>
    /// <param name="session">
    ///     The session.
    /// </param>
    /// <param name="collection">
    ///     The collection.
    /// </param>
    public PersistentObservableCollection(ISessionImplementor session, IList<T> collection) :
        base(session, collection)
    {
        if (collection is not null)
        {
            ((INotifyCollectionChanged) collection).CollectionChanged += OnCollectionChanged;
        }
    }

    /// <summary>
    ///     Occurs when the collection changes.
    /// </summary>
    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///     Called when the collection changes.
    /// </summary>
    /// <param name="sender">
    ///     The sender.
    /// </param>
    /// <param name="args">
    ///     The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.
    /// </param>
    protected void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        CollectionChanged?.Invoke(this, args);
    }

    /// <summary>
    ///     Before the initialize.
    /// </summary>
    /// <param name="persister">
    ///     The persister.
    /// </param>
    /// <param name="anticipatedSize">
    ///     Size of the anticipated.
    /// </param>
    public override void BeforeInitialize(ICollectionPersister persister, int anticipatedSize)
    {
        base.BeforeInitialize(persister, anticipatedSize);

        ((INotifyCollectionChanged) WrappedList).CollectionChanged += OnCollectionChanged;
    }
}
