using System.ComponentModel;

using NHibernate.ObservableCollections.Helpers.BidirectionalAssociations;

namespace NHibernate.ObservableCollections.DemoApp
{
    public class SampleItem : IEquatable<SampleItem>, INotifyPropertyChanged
    {
        private string _name = string.Empty;

        private SampleSetContainer _parentSetContainer;

        public virtual event PropertyChangedEventHandler PropertyChanged;

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

        public virtual SampleSetContainer ParentSetContainer
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

        public virtual bool Equals(SampleItem other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            //return Id == other.Id && Name == other.Name;
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SampleItem);
        }

        public override int GetHashCode()
        {
            //return HashCode.Combine(Id, Name);
            return HashCode.Combine(Name);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
