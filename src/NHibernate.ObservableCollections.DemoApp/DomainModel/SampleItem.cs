namespace NHibernate.ObservableCollections.DemoApp
{
    #region Using Directives

    using System;
    using System.ComponentModel;

    using NHibernate.ObservableCollections.Helpers.BidirectionalAssociations;

    #endregion

    public class SampleItem : INotifyPropertyChanged
    {
        private string _name;

        private SampleSetContainer _parentSetContainer;

        public virtual int Id { get; protected set; }

        public virtual string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public virtual SampleSetContainer ParentSetContainer
        {
            get => this._parentSetContainer;
            set
            {
                Console.WriteLine("setting sample item's parent set container");
                var oldParentSetContainer = this._parentSetContainer;
                this._parentSetContainer = value;
                OneToManyAssocSync.UpdateOneSide(this, oldParentSetContainer, this._parentSetContainer, "SampleSet");
            }
        }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}