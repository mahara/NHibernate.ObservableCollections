using System.ComponentModel;

namespace Iesi.Collections.Generic.Tests;

internal class TestObservableSet<T> : ObservableSet<T>
{
    public TestObservableSet()
    {
    }

    public TestObservableSet(IEnumerable<T> collection) :
        base(collection)
    {
    }

    public List<NotifyCollectionChangedEventArgs> CollectionChangedEventArgsList { get; } = [];

    public List<PropertyChangedEventArgs> PropertyChangedEventArgsList { get; } = [];

    public void ResetAllArgsLists()
    {
        CollectionChangedEventArgsList.Clear();

        PropertyChangedEventArgsList.Clear();
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);

        CollectionChangedEventArgsList.Add(e);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        PropertyChangedEventArgsList.Add(e);
    }
}
