using System.ComponentModel;

namespace Iesi.Collections.Generic.Tests;

internal class TestObservableCollection<T> : ObservableCollection<T>
{
    public TestObservableCollection()
    {
    }

    public TestObservableCollection(IEnumerable<T> collection) :
        base(collection)
    {
    }

    public TestObservableCollection(List<T> collection) :
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

        if (!EventNotificationsAreDeferred)
        {
            CollectionChangedEventArgsList.Add(e);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!EventNotificationsAreDeferred)
        {
            PropertyChangedEventArgsList.Add(e);
        }
    }
}

internal class TestReadOnlyObservableCollection<T> : ReadOnlyObservableCollection<T>
{
    public TestReadOnlyObservableCollection(ObservableCollection<T> collection) :
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
