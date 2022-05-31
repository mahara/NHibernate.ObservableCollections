namespace NHibernate.ObservableCollections.DemoApp
{
    using System.Windows;
    using System.Windows.Controls;

    using NHibernate.ObservableCollections.DemoApp.DataAccess;

    public partial class MainWindow
    {
        private SampleListContainer _sampleListContainer = new();

        private SampleSetContainer _sampleSetContainer = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool GetSelection(out object? selectedContainer, out SampleItem? selectedItem)
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

        private void ItemSelectionChange(object s, RoutedEventArgs e)
        {
            var sender = (ListBox) s;
            if (sender.SelectedItem == null)
            {
                return;
            }

            var selectedItem = (SampleItem) sender.SelectedItem;
            editItemBox.Text = selectedItem.Name; // display item name in text box
            (sender == SampleSetBox ? SampleListBox : SampleSetBox).UnselectAll(); // deselect in other box
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new SampleItem();
            newItem.Name = editItemBox.Text; // add new item from value in text box
            if (_sampleSetContainer.SampleSet.Add(newItem))
            {
                newItem.ParentSetContainer = _sampleSetContainer;
                using var dbMgr = new NHibernateDbMgr();
                dbMgr.Save(newItem);
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
            using var dbMgr = new NHibernateDbMgr();
            dbMgr.Update(_sampleListContainer);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GetSelection(out var selectedContainer, out var selectedItem))
            {
                return;
            }

            if (selectedContainer is SampleSetContainer)
            {
                // then delete item from set
                if (selectedItem != null && _sampleSetContainer.SampleSet.Remove(selectedItem))
                {
                    selectedItem.ParentSetContainer = null;

                    using var dbMgr = new NHibernateDbMgr();
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
                // delete item from list
                if (selectedItem != null && _sampleListContainer.SampleList.Remove(selectedItem))
                {
                    if (selectedContainer != null)
                    {
                        using var dbMgr = new NHibernateDbMgr();
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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // load the set and list containers from database, then bind their items to list boxes
            using var dbMgr = new NHibernateDbMgr();
            _sampleSetContainer = dbMgr.Get<SampleSetContainer>(1);
            SampleSetBox.ItemsSource = _sampleSetContainer.SampleSet;
            _sampleListContainer = dbMgr.Get<SampleListContainer>(2);
            SampleListBox.ItemsSource = _sampleListContainer.SampleList;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!GetSelection(out _, out var selectedItem))
            {
                return;
            }

            if (selectedItem != null)
            {
                selectedItem.Name = editItemBox.Text; // update selected item from value in text box
                using var dbMgr = new NHibernateDbMgr();
                dbMgr.Update(selectedItem);
            }
        }
    }
}
