using System.Windows;
using System.Windows.Controls;

using NHibernate.ObservableCollections.DemoApp.DataAccess;

namespace NHibernate.ObservableCollections.DemoApp
{
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
            try
            {
                // Load the set and list containers from database, then bind their items to list boxes.
                using var dbManager = new NHibernateDatabaseManager();

                _sampleSetContainer = dbManager.Get<SampleSetContainer>(1);
                SampleSetBox.ItemsSource = _sampleSetContainer.SampleSet;

                _sampleListContainer = dbManager.Get<SampleListContainer>(2);
                SampleListBox.ItemsSource = _sampleListContainer.SampleList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ItemSelectionChanged(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox) sender;
            if (listBox.SelectedItem == null)
            {
                return;
            }

            var selectedItem = (SampleItem) listBox.SelectedItem;
            EditItemBox.Text = selectedItem.Name; // Display item name in text box.
            (listBox == SampleSetBox ? SampleListBox : SampleSetBox).UnselectAll(); // Deselect in other box.
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new SampleItem();
            newItem.Name = EditItemBox.Text; // Add new item from value in text box.

            if (_sampleSetContainer.SampleSet.Add(newItem))
            {
                newItem.ParentSetContainer = _sampleSetContainer;

                using var dbManager = new NHibernateDatabaseManager();

                dbManager.Save(newItem);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetSelection(out _, out var selectedItem))
            {
                return;
            }

            if (selectedItem != null)
            {
                selectedItem.Name = EditItemBox.Text; // Update selected item from value in text box.

                using var dbManager = new NHibernateDatabaseManager();

                dbManager.Update(selectedItem);
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
                if (selectedItem != null && _sampleSetContainer.SampleSet.Remove(selectedItem))
                {
                    selectedItem.ParentSetContainer = null;

                    using var dbManager = new NHibernateDatabaseManager();

                    if (_sampleListContainer.SampleList.Contains(selectedItem))
                    {
                        dbManager.Update(selectedItem);
                    }
                    else
                    {
                        dbManager.Delete(selectedItem);
                    }
                }
            }
            else
            {
                // Delete item from list.
                if (selectedItem != null && _sampleListContainer.SampleList.Remove(selectedItem))
                {
                    if (selectedContainer != null)
                    {
                        using var dbManager = new NHibernateDatabaseManager();

                        dbManager.Update(selectedContainer);

                        if (!(_sampleSetContainer.SampleSet.Contains(selectedItem) ||
                              _sampleListContainer.SampleList.Contains(selectedItem)))
                        {
                            dbManager.Delete(selectedItem);
                        }
                    }
                }
            }
        }

        private void CopyToButton_Click(object sender, RoutedEventArgs e)
        {
            if (SampleSetBox.SelectedItem == null)
            {
                return;
            }

            var selectedItem = (SampleItem) SampleSetBox.SelectedItem;
            _sampleListContainer.SampleList.Add(selectedItem);

            using var dbManager = new NHibernateDatabaseManager();

            dbManager.Update(_sampleListContainer);
        }

        private bool TryGetSelection(out object selectedContainer, out SampleItem selectedItem)
        {
            if (SampleSetBox.SelectedItem != null)
            {
                selectedContainer = _sampleSetContainer;
                selectedItem = (SampleItem) SampleSetBox.SelectedItem;

                return true;
            }

            if (SampleListBox.SelectedItem != null)
            {
                selectedContainer = _sampleListContainer;
                selectedItem = (SampleItem) SampleListBox.SelectedItem;

                return true;
            }

            selectedContainer = null;
            selectedItem = null;

            return false;
        }
    }
}
