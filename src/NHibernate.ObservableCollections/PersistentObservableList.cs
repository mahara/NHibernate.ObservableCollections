namespace NHibernate.Collection.Generic
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;

    using NHibernate.DebugHelpers;
    using NHibernate.Engine;
    using NHibernate.Persister.Collection;

    #endregion

    /// <summary>
    ///     Represents a persistent observable list.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the items in the list.
    /// </typeparam>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionProxy<>))]
    public class PersistentObservableList<T> : PersistentGenericList<T>, INotifyCollectionChanged
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentObservableList{T}" /> class.
        /// </summary>
        /// <param name="session">
        ///     The session.
        /// </param>
        public PersistentObservableList(ISessionImplementor session)
            : base(session)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentObservableList{T}" /> class.
        /// </summary>
        /// <param name="session">
        ///     The session.
        /// </param>
        /// <param name="collection">
        ///     The collection.
        /// </param>
        public PersistentObservableList(ISessionImplementor session, IList<T> collection)
            : base(session, collection)
        {
            if (collection != null)
            {
                ((INotifyCollectionChanged) collection).CollectionChanged += this.OnCollectionChanged;
            }
        }

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler CollectionChanged;

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
            ((INotifyCollectionChanged) this.WrappedList).CollectionChanged += this.OnCollectionChanged;
        }

        /// <summary>
        ///     Called when [collection changed].
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="args">
        ///     The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            this.CollectionChanged?.Invoke(this, args);
        }
    }
}