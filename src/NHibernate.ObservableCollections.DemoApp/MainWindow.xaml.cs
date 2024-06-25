using System.Windows;
using System.Windows.Controls;

using NHibernate.ObservableCollections.DemoApp.DataAccess;

namespace NHibernate.ObservableCollections.DemoApp;

public partial class MainWindow
{
    private SampleSetContainer _sampleSetContainer = new();
    private SampleListContainer _sampleListContainer = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Load the set and list containers from database, then bind their items to list boxes.
        using var dbMgr = new NHibernateDatabaseManager();

        _sampleSetContainer = dbMgr.Get<SampleSetContainer>(1);
        NHibernateUtil.Initialize(_sampleSetContainer.SampleSet);
        SampleSetBox.ItemsSource = _sampleSetContainer.SampleSet;

        _sampleListContainer = dbMgr.Get<SampleListContainer>(2);
        NHibernateUtil.Initialize(_sampleListContainer.SampleList);
        SampleListBox.ItemsSource = _sampleListContainer.SampleList;
    }

    private void ItemSelectionChange(object s, RoutedEventArgs e)
    {
        var sender = (ListBox) s;
        if (sender.SelectedItem is null)
        {
            return;
        }

        var selectedItem = (SampleItem) sender.SelectedItem;
        EditItemBox.Text = selectedItem.Name; // Display item name in text box.
        (sender == SampleSetBox ? SampleListBox : SampleSetBox).UnselectAll(); // Deselect in other box.
    }

    private bool TryGetSelection(out object? selectedContainer, out SampleItem? selectedItem)
    {
        if (SampleSetBox.SelectedItem is not null)
        {
            selectedContainer = _sampleSetContainer;
            selectedItem = (SampleItem) SampleSetBox.SelectedItem;
            return true;
        }

        if (SampleListBox.SelectedItem is not null)
        {
            selectedContainer = _sampleListContainer;
            selectedItem = (SampleItem) SampleListBox.SelectedItem;
            return true;
        }

        selectedContainer = null;
        selectedItem = null;
        return false;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var newItem = new SampleItem
        {
            Name = EditItemBox.Text // Add new item from value in text box.
        };
        if (_sampleSetContainer.SampleSet.Add(newItem))
        {
            newItem.ParentSetContainer = _sampleSetContainer;

            using var dbMgr = new NHibernateDatabaseManager();

            dbMgr.Save(newItem);
        }
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out _, out var selectedItem))
        {
            return;
        }

        if (selectedItem is not null)
        {
            selectedItem.Name = EditItemBox.Text; // Update selected item from value in text box.

            using var dbMgr = new NHibernateDatabaseManager();

            dbMgr.Update(selectedItem);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out var selectedContainer, out var selectedItem))
        {
            return;
        }

        if (selectedContainer is SampleSetContainer)
        {
            // Delete item from set.
            if (selectedItem is not null && _sampleSetContainer.SampleSet.Remove(selectedItem))
            {
                selectedItem.ParentSetContainer = null;

                using var dbMgr = new NHibernateDatabaseManager();

                if (_sampleListContainer.SampleList.Contains(selectedItem))
                {
                    dbMgr.Update(selectedItem);
                }
                else
                {
                    dbMgr.Delete(selectedItem);
                }
            }
        }
        else
        {
            // Delete item from list.
            if (selectedItem is not null && _sampleListContainer.SampleList.Remove(selectedItem))
            {
                if (selectedContainer is not null)
                {
                    using var dbMgr = new NHibernateDatabaseManager();

                    dbMgr.Update(selectedContainer);

                    if (!(_sampleSetContainer.SampleSet.Contains(selectedItem) ||
                          _sampleListContainer.SampleList.Contains(selectedItem)))
                    {
                        dbMgr.Delete(selectedItem);
                    }
                }
            }
        }
    }

    private void CopyToButton_Click(object sender, RoutedEventArgs e)
    {
        if (SampleSetBox.SelectedItem is null)
        {
            return;
        }

        var selectedItem = (SampleItem) SampleSetBox.SelectedItem;
        _sampleListContainer.SampleList.Add(selectedItem);

        using var dbMgr = new NHibernateDatabaseManager();

        dbMgr.Update(_sampleListContainer);
    }
}
