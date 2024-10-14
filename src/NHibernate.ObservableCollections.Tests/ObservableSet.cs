using System.ComponentModel;

namespace Iesi.Collections.Generic.Tests
{
    public class TestObservableSet<T> : ObservableSet<T>
    {
        public TestObservableSet()
        {
        }

        public TestObservableSet(IEnumerable<T> collection) :
            base(collection)
        {
        }

        public TestObservableSet(IEqualityComparer<T> comparer) :
            base(comparer)
        {
        }

        public TestObservableSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) :
            base(collection, comparer)
        {
        }

        public List<NotifyCollectionChangedEventArgs> CollectionChangedEventArgsList { get; } = [];

        public List<PropertyChangingEventArgs> PropertyChangingEventArgsList { get; } = [];

        public List<PropertyChangedEventArgs> PropertyChangedEventArgsList { get; } = [];

        public void ResetAllEventArgsLists()
        {
            CollectionChangedEventArgsList.Clear();

            PropertyChangingEventArgsList.Clear();

            PropertyChangedEventArgsList.Clear();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            CollectionChangedEventArgsList.Add(e);
        }

        protected override void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            base.OnPropertyChanging(e);

            PropertyChangingEventArgsList.Add(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            PropertyChangedEventArgsList.Add(e);
        }
    }
}
