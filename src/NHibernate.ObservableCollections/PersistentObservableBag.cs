using System.Collections.Specialized;
using System.Diagnostics;

using NHibernate.Collection.Generic;
using NHibernate.DebugHelpers;
using NHibernate.Engine;
using NHibernate.Persister.Collection;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     Represents a persistent observable bag.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the items in the bag.
    /// </typeparam>
    /// <remarks>
    ///     AUTHORS:
    ///     -   Adrian Alexander
    ///     REFERENCES:
    ///     -   <see href="https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2008/05/revisiting-bidirectional-assoc-helpers.html" />
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionProxy<>))]
    public class PersistentObservableBag<T> : PersistentGenericBag<T>, INotifyCollectionChanged
    {
        public PersistentObservableBag(ISessionImplementor session) :
            base(session)
        {
        }

        public PersistentObservableBag(ISessionImplementor session, ICollection<T> collection) :
            base(session, collection)
        {
            if (collection != null)
            {
                ((INotifyCollectionChanged) collection).CollectionChanged += OnCollectionChanged;
            }
        }

        /// <summary>
        ///     Occurs when an item is added, removed, or moved, or the entire bag is refreshed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc />
        public override void BeforeInitialize(ICollectionPersister persister, int anticipatedSize)
        {
            base.BeforeInitialize(persister, anticipatedSize);

            ((INotifyCollectionChanged) InternalBag).CollectionChanged += OnCollectionChanged;
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }
    }
}
