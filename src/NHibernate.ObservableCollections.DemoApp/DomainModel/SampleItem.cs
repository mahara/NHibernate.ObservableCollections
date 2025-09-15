using NHibernate.ObservableCollections.Helpers.BidirectionalAssociations;

namespace NHibernate.ObservableCollections.DemoApp
{
    public class SampleItem : INotifyPropertyChanged
    {
        private string _name = string.Empty;

        private SampleSetContainer? _parentSetContainer;

        public virtual event PropertyChangedEventHandler? PropertyChanged;

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
                OneToManyAssociationSync.UpdateOneSide(this, oldParentSetContainer, _parentSetContainer, "SampleSet");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
