namespace NHibernate.ObservableCollections.Helpers.BidirectionalAssociations
{
    #region Using Directives

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;

    #endregion

    /// <summary>
    ///     Keeps both sides of a bidirectional one-to-many association in sync with each other.
    /// </summary>
    /// <author>Adrian Alexander</author>
    public sealed class OneToManyAssocSync
    {
        private readonly string _manyToOnePropertyName;

        private readonly object _thisOneSide;

        private PropertyInfo _manyToOneProperty;

        /// <param name="thisOneSide">The entity participating in the association which has a multiplicity of one</param>
        /// <param name="manyToOnePropertyName"></param>
        public OneToManyAssocSync(object thisOneSide, string manyToOnePropertyName)
        {
            this._thisOneSide = thisOneSide;
            this._manyToOnePropertyName = manyToOnePropertyName;
        }

        /// <summary>
        ///     Responds to a many-side entity's parent one-side property being set to a new value.
        /// </summary>
        public static void UpdateOneSide<T>(T thisManySide, object oldOneSide, object newOneSide, string oneToManyPropertyName)
        {
            if (oldOneSide != null && oldOneSide != newOneSide)
            {
                var oldCollection =
                    ReflectionUtil.NavigateToManySide<T>(oldOneSide, oneToManyPropertyName);
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

            if (newOneSide != null)
            {
                var newCollection =
                    ReflectionUtil.NavigateToManySide<T>(newOneSide, oneToManyPropertyName);
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
        public void UpdateManySide(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //addingToManySide: the item that was just added to this one-side's collection
                foreach (var addingToManySide in e.NewItems)
                {
                    if (addingToManySide != null && this.NavigateManyToOne(addingToManySide) != this._thisOneSide)
                    {
                        this.SetManyToOne(addingToManySide, this._thisOneSide);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //removingFromManySide: the item that was just removed from this one-side's collection
                foreach (var removingFromManySide in e.OldItems)
                {
                    if (this.NavigateManyToOne(removingFromManySide) == this._thisOneSide)
                    {
                        this.SetManyToOne(removingFromManySide, null);
                    }
                }
            }
        }

        private PropertyInfo GetManyToOneProperty(Type manySideType)
        {
            if (this._manyToOneProperty == null)
            {
                var pi = manySideType.GetProperty(this._manyToOnePropertyName);
                this._manyToOneProperty = pi.DeclaringType.GetProperty(this._manyToOnePropertyName);
            }

            return this._manyToOneProperty;
        }

        private object NavigateManyToOne(object manySide)
        {
            return this.GetManyToOneProperty(manySide.GetType()).GetValue(manySide, null);
        }

        private void SetManyToOne(object manySide, object newValue)
        {
            this.GetManyToOneProperty(manySide.GetType()).SetValue(manySide, newValue, null);
        }
    }
}