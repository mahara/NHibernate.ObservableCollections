namespace NHibernate.ObservableCollections.DemoApp
{
    using System.Collections.Generic;
    using System.Collections.Specialized;

    using Iesi.Collections.Generic;

    using NHibernate.ObservableCollections.Helpers.BidirectionalAssociations;

    /// <summary>
    ///     A parent class that contains a set of child <see cref="SampleItem" /> objects.
    /// </summary>
    public class SampleSetContainer
    {
        private ISet<SampleItem> _sampleSet = new ObservableSet<SampleItem>();

        public virtual int Id { get; protected set; }

        public virtual ISet<SampleItem> SampleSet
        {
            get => _sampleSet;
            protected set
            {
                _sampleSet = value;
                ((INotifyCollectionChanged) _sampleSet).CollectionChanged +=
                    new OneToManyAssocSync(this, "ParentSetContainer").UpdateManySide;
            }
        }
    }
}