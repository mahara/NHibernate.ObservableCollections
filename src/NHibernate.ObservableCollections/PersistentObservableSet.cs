namespace Iesi.Collections.Generic
{
    using System.Diagnostics;

    using NHibernate.DebugHelpers;
    using NHibernate.Engine;
    using NHibernate.Persister.Collection;

    /// <summary>
    ///     Represents a persistent observable set.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the items in the set.
    /// </typeparam>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionProxy<>))]
    public class PersistentObservableSet<T> : PersistentGenericSet<T>, INotifyCollectionChanged
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentObservableSet{T}" /> class.
        /// </summary>
        /// <param name="session">
        ///     The session.
        /// </param>
        public PersistentObservableSet(ISessionImplementor session) :
            base(session)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentObservableSet{T}" /> class.
        /// </summary>
        /// <param name="session">
        ///     The session.
        /// </param>
        /// <param name="collection">
        ///     The collection.
        /// </param>
        public PersistentObservableSet(ISessionImplementor session, ISet<T> collection) :
            base(session, collection)
        {
            if (collection != null)
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

            ((INotifyCollectionChanged) WrappedSet).CollectionChanged += OnCollectionChanged;
        }

        /// <summary>
        ///     Called when CollectionChanged.
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
    }
}
