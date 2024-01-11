namespace NHibernate.ObservableCollections.DemoApp
{
    /// <summary>
    ///     A parent class that contains a list of child <see cref="SampleItem" /> objects.
    /// </summary>
    public class SampleListContainer
    {
        private IList<SampleItem> _sampleList = new ObservableCollection<SampleItem>();

        public virtual int Id { get; protected set; }

        public virtual IList<SampleItem> SampleList
        {
            get => _sampleList;
            protected set
            {
                _sampleList = value;

                ((INotifyCollectionChanged) _sampleList).CollectionChanged += OnSampleListCollectionChanged;
            }
        }

        private void OnSampleListCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newItems = e.NewItems;
                if (newItems is not null)
                {
                    foreach (SampleItem item in newItems)
                    {
                        Console.WriteLine($"{item} added to the list");
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var oldItems = e.OldItems;
                if (oldItems is not null)
                {
                    foreach (SampleItem item in oldItems)
                    {
                        Console.WriteLine($"{item} removed from the list");
                    }
                }
            }
        }
    }
}
