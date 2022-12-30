namespace NHibernate.ObservableCollections.Helpers.BidirectionalAssociations
{
    using System.Reflection;

    /// <summary>
    ///     Keeps both sides of a bidirectional many-to-many association in sync with each other.
    /// </summary>
    /// <author>Adrian Alexander</author>
    public sealed class ManyToManyAssociationSync<T>
    {
        private readonly T _thisSide;

        private readonly string _otherSidePropertyName;

        private PropertyInfo? _otherSideProperty;

        public ManyToManyAssociationSync(T thisSide, string otherSideToThisSidePropertyName)
        {
            _thisSide = thisSide;
            _otherSidePropertyName = otherSideToThisSidePropertyName;
        }

        /// <summary>
        ///     Responds to add/remove events raised by this side's collection.
        /// </summary>
        public void UpdateOtherSide(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // addingToOtherSide: the item that was just added to this side's collection
                foreach (var addingToOtherSide in e.NewItems!)
                {
                    Console.WriteLine("new item added to set");

                    var otherSidesCollection = GetOtherSidesCollection(addingToOtherSide);

                    if (otherSidesCollection is not null &&
                        ReflectionUtil.IsInitialized(otherSidesCollection) &&
                        !otherSidesCollection.Contains(_thisSide))
                    {
                        if (otherSidesCollection is IList && otherSidesCollection.Contains(_thisSide))
                        {
                            // true if add will cause duplicates
                            throw new InvalidOperationException("Collection is non-unique");
                        }

                        otherSidesCollection.Add(_thisSide);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // removingFromOtherSide: the item that was just removed from this side's collection
                foreach (var removingFromOtherSide in e.OldItems!)
                {
                    Console.WriteLine("old item removed from set");

                    var otherSidesCollection = GetOtherSidesCollection(removingFromOtherSide);

                    if (otherSidesCollection is not null &&
                        ReflectionUtil.IsInitialized(otherSidesCollection) &&
                        otherSidesCollection.Contains(_thisSide))
                    {
                        otherSidesCollection.Remove(_thisSide);
                        if (otherSidesCollection is IList && otherSidesCollection.Contains(_thisSide))
                        {
                            // true if there was more than one
                            throw new InvalidOperationException("Collection is non-unique");
                        }
                    }
                }
            }
        }

        private ICollection<T>? GetOtherSidesCollection(object otherSide)
        {
            if (_otherSideProperty == null)
            {
                _otherSideProperty = otherSide.GetType().GetProperty(_otherSidePropertyName);
            }

            var otherSideProperty = _otherSideProperty;
            if (otherSideProperty != null)
            {
                return (ICollection<T>?) otherSideProperty.GetValue(otherSide, null);
            }

            return null;
        }
    }
}
