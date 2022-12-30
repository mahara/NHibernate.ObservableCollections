namespace NHibernate.ObservableCollections.DemoApp
{
    using System.ComponentModel;

    using NHibernate.ObservableCollections.Helpers.BidirectionalAssociations;

    public class SampleItem : INotifyPropertyChanged
    {
        private string _name = string.Empty;

        private SampleSetContainer? _parentSetContainer;

        public virtual int Id { get; protected set; }

        public virtual string Name
        {
            get => _name;
            set
            {
                _name = value;

                OnPropertyChanged(nameof(Name));
            }
        }

        public virtual SampleSetContainer? ParentSetContainer
        {
            get => _parentSetContainer;
            set
            {
                Console.WriteLine("setting sample item's parent set container");
                var oldParentSetContainer = _parentSetContainer;
                _parentSetContainer = value;
                OneToManyAssociationSync.UpdateOneSide(this, oldParentSetContainer!, _parentSetContainer!, "SampleSet");
            }
        }

        public virtual event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
