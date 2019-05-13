namespace NHibernate.ObservableCollections.DemoApp
{
    #region Using Directives

    using System.Windows;
    using System.Windows.Controls;

    using NHibernate.ObservableCollections.DemoApp.DataAccess;

    #endregion

    public partial class MainWindow
    {
        private SampleListContainer _sampleListContainer;

        private SampleSetContainer _sampleSetContainer;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private bool GetSelection(out object selectedContainer, out SampleItem selectedItem)
        {
            if (this.sampleSetBox.SelectedItem != null)
            {
                selectedContainer = this._sampleSetContainer;
                selectedItem = (SampleItem) this.sampleSetBox.SelectedItem;
                return true;
            }

            if (this.sampleListBox.SelectedItem != null)
            {
                selectedContainer = this._sampleListContainer;
                selectedItem = (SampleItem) this.sampleListBox.SelectedItem;
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
            this.editItemBox.Text = selectedItem.Name; // display item name in text box
            (sender == this.sampleSetBox ? this.sampleListBox : this.sampleSetBox).UnselectAll(); // deselect in other box
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new SampleItem();
            newItem.Name = this.editItemBox.Text; // add new item from value in text box
            if (this._sampleSetContainer.SampleSet.Add(newItem))
            {
                newItem.ParentSetContainer = this._sampleSetContainer;
                using (var dbMgr = new NHibernateDbMgr())
                {
                    dbMgr.Save(newItem);
                }
            }
        }

        private void copyToButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.sampleSetBox.SelectedItem == null)
            {
                return;
            }

            var selectedItem = (SampleItem) this.sampleSetBox.SelectedItem;
            this._sampleListContainer.SampleList.Add(selectedItem);
            using (var dbMgr = new NHibernateDbMgr())
            {
                dbMgr.Update(this._sampleListContainer);
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.GetSelection(out var selectedContainer, out var selectedItem))
            {
                return;
            }

            if (selectedContainer is SampleSetContainer)
            {
                // then delete item from set
                if (this._sampleSetContainer.SampleSet.Remove(selectedItem))
                {
                    selectedItem.ParentSetContainer = null;
                    using (var dbMgr = new NHibernateDbMgr())
                    {
                        if (this._sampleListContainer.SampleList.Contains(selectedItem))
                        {
                            dbMgr.Update(selectedItem);
                        }
                        else
                        {
                            dbMgr.Delete(selectedItem);
                        }
                    }
                }
            }
            else
            {
                // delete item from list
                if (this._sampleListContainer.SampleList.Remove(selectedItem))
                {
                    using (var dbMgr = new NHibernateDbMgr())
                    {
                        dbMgr.Update(selectedContainer);
                        if (!(this._sampleSetContainer.SampleSet.Contains(selectedItem) ||
                              this._sampleListContainer.SampleList.Contains(selectedItem)))
                        {
                            dbMgr.Delete(selectedItem);
                        }
                    }
                }
            }
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            // load the set and list containers from database, then bind their items to list boxes
            using (var dbMgr = new NHibernateDbMgr())
            {
                this._sampleSetContainer = dbMgr.Get<SampleSetContainer>(1);
                this.sampleSetBox.ItemsSource = this._sampleSetContainer.SampleSet;
                this._sampleListContainer = dbMgr.Get<SampleListContainer>(2);
                this.sampleListBox.ItemsSource = this._sampleListContainer.SampleList;
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.GetSelection(out _, out var selectedItem))
            {
                return;
            }

            selectedItem.Name = this.editItemBox.Text; // update selected item from value in text box
            using (var dbMgr = new NHibernateDbMgr())
            {
                dbMgr.Update(selectedItem);
            }
        }
    }
}