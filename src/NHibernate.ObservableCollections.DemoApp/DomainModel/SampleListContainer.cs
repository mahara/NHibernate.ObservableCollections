namespace NHibernate.ObservableCollections.DemoApp
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    #endregion

    /// <summary>
    ///     A parent class that contains a list of child <see cref="SampleItem" /> objects.
    /// </summary>
    public class SampleListContainer
    {
        private IList<SampleItem> _sampleList;

        public virtual int Id { get; protected set; }

        public virtual IList<SampleItem> SampleList
        {
            get => this._sampleList;
            protected set
            {
                this._sampleList = value;
                ((INotifyCollectionChanged) this._sampleList).CollectionChanged += this.value_CollectionChanged;
            }
        }

        private void value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (SampleItem item in e.NewItems)
                {
                    Console.WriteLine("new item added to the list");
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (SampleItem item in e.OldItems)
                {
                    Console.WriteLine("item removed from the list");
                }
            }
        }
    }
}