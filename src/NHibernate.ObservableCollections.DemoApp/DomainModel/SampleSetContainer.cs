namespace NHibernate.ObservableCollections.DemoApp
{
    #region Using Directives

    using System.Collections.Generic;
    using System.Collections.Specialized;

    using NHibernate.ObservableCollections.Helpers.BidirectionalAssociations;

    #endregion

    /// <summary>
    ///     A parent class that contains a set of child <see cref="SampleItem" /> objects.
    /// </summary>
    public class SampleSetContainer
    {
        private ISet<SampleItem> _sampleSet;

        public virtual int Id { get; protected set; }

        public virtual ISet<SampleItem> SampleSet
        {
            get => this._sampleSet;
            protected set
            {
                this._sampleSet = value;
                ((INotifyCollectionChanged) this._sampleSet).CollectionChanged +=
                    new OneToManyAssocSync(this, "ParentSetContainer").UpdateManySide;
            }
        }
    }
}