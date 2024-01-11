namespace NHibernate.ObservableCollections.Helpers.BidirectionalAssociations
{
    using System.Reflection;

    /// <summary>
    ///     Keeps both sides of a bidirectional one-to-many association in sync with each other.
    /// </summary>
    /// <remarks>
    ///     AUTHORS:
    ///     -   Adrian Alexander
    ///     REFERENCES:
    ///     -   <see href="https://happynomad121.blogspot.com/2007/12/collections-for-wpf-and-nhibernate.html" />
    ///     -   <see href="https://happynomad121.blogspot.com/2008/05/revisiting-bidirectional-assoc-helpers.html" />
    /// </remarks>
    public sealed class OneToManyAssociationSync
    {
        private readonly object _thisOneSide;

        private readonly string _manyToOnePropertyName;

        private PropertyInfo? _manyToOneProperty;

        /// <summary>
        ///    Initializes a new instance of the <see cref="OneToManyAssocSync" /> class.
        /// </summary>
        /// <param name="thisOneSide">The entity participating in the association which has a multiplicity of one.</param>
        /// <param name="manyToOnePropertyName"></param>
        public OneToManyAssociationSync(object thisOneSide, string manyToOnePropertyName)
        {
            _thisOneSide = thisOneSide;
            _manyToOnePropertyName = manyToOnePropertyName;
        }

        /// <summary>
        ///     Responds to a many-side entity's parent one-side property being set to a new value.
        /// </summary>
        public static void UpdateOneSide<T>(T thisManySide, object oldOneSide, object newOneSide, string oneToManyPropertyName)
        {
            if (oldOneSide is not null && oldOneSide != newOneSide)
            {
                var oldCollection = ReflectionUtil.NavigateToManySide<T>(oldOneSide, oneToManyPropertyName);
                if (oldCollection.Contains(thisManySide))
                {
                    Console.WriteLine("\tremoving from old parent collection");

                    oldCollection.Remove(thisManySide);

                    if (oldCollection is IList && oldCollection.Contains(thisManySide))
                    {
                        // true if there was more than one
                        throw new InvalidOperationException("Collection is non-unique");
                    }
                }
            }

            if (newOneSide is not null)
            {
                var newCollection = ReflectionUtil.NavigateToManySide<T>(newOneSide, oneToManyPropertyName);
                if (ReflectionUtil.IsInitialized(newCollection) && !newCollection.Contains(thisManySide))
                {
                    Console.WriteLine("\tadding to new parent collection");

                    if (newCollection is IList && newCollection.Contains(thisManySide))
                    {
                        // true if add will cause duplicates
                        throw new InvalidOperationException("Collection is non-unique");
                    }

                    newCollection.Add(thisManySide);
                }
            }
        }

        /// <summary>
        ///     Responds to add/remove events raised by the one-side's collection.
        /// </summary>
        public void UpdateManySide(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // addingToManySide: the item that was just added to this one-side's collection
                foreach (var addingToManySide in e.NewItems!)
                {
                    if (addingToManySide is not null && NavigateManyToOne(addingToManySide) != _thisOneSide)
                    {
                        SetManyToOne(addingToManySide, _thisOneSide);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // removingFromManySide: the item that was just removed from this one-side's collection
                foreach (var removingFromManySide in e.OldItems!)
                {
                    if (NavigateManyToOne(removingFromManySide) == _thisOneSide)
                    {
                        SetManyToOne(removingFromManySide, null);
                    }
                }
            }
        }

        private PropertyInfo GetManyToOneProperty(System.Type manySideType)
        {
            if (_manyToOneProperty is null)
            {
                var pi = manySideType.GetProperty(_manyToOnePropertyName)!;
                _manyToOneProperty = pi.DeclaringType!.GetProperty(_manyToOnePropertyName)!;
            }

            return _manyToOneProperty;
        }

        private object NavigateManyToOne(object manySide)
        {
            return GetManyToOneProperty(manySide.GetType()).GetValue(manySide, null)!;
        }

        private void SetManyToOne(object manySide, object? newValue)
        {
            GetManyToOneProperty(manySide.GetType()).SetValue(manySide, newValue, null);
        }
    }
}
