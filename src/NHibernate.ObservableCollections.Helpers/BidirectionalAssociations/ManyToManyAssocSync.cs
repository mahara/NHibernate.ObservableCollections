namespace NHibernate.ObservableCollections.Helpers.BidirectionalAssociations
{
    #region Using Directives

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Reflection;

    #endregion

    /// <summary>
    ///     Keeps both sides of a bidirectional many-to-many association in sync with each other.
    /// </summary>
    /// <author>Adrian Alexander</author>
    public sealed class ManyToManyAssocSync<T>
    {
        private readonly string _otherSidePropertyName;

        private readonly T _thisSide;

        private PropertyInfo _otherSideProperty;

        public ManyToManyAssocSync(T thisSide, string otherSideToThisSidePropertyName)
        {
            this._thisSide = thisSide;
            this._otherSidePropertyName = otherSideToThisSidePropertyName;
        }

        /// <summary>
        ///     Responds to add/remove events raised by this side's collection.
        /// </summary>
        public void UpdateOtherSide(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //addingToOtherSide: the item that was just added to this side's collection
                foreach (var addingToOtherSide in e.NewItems)
                {
                    Console.WriteLine("new item added to set");
                    var otherSidesCollection = this.GetOtherSidesCollection(addingToOtherSide);
                    if (ReflectionUtil.IsInitialized(otherSidesCollection) && !otherSidesCollection.Contains(this._thisSide))
                    {
                        if (otherSidesCollection is IList && otherSidesCollection.Contains(this._thisSide))
                        {
                            // true if add will cause duplicates
                            throw new InvalidOperationException("Collection is non-unique");
                        }

                        otherSidesCollection.Add(this._thisSide);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //removingFromOtherSide: the item that was just removed from this side's collection
                foreach (var removingFromOtherSide in e.OldItems)
                {
                    Console.WriteLine("old item removed from set");
                    var otherSidesCollection = this.GetOtherSidesCollection(removingFromOtherSide);
                    if (ReflectionUtil.IsInitialized(otherSidesCollection) && otherSidesCollection.Contains(this._thisSide))
                    {
                        otherSidesCollection.Remove(this._thisSide);
                        if (otherSidesCollection is IList && otherSidesCollection.Contains(this._thisSide))
                        {
                            // true if there was more than one
                            throw new InvalidOperationException("Collection is non-unique");
                        }
                    }
                }
            }
        }

        private ICollection<T> GetOtherSidesCollection(object otherSide)
        {
            if (this._otherSideProperty == null)
            {
                this._otherSideProperty = otherSide.GetType().GetProperty(this._otherSidePropertyName);
            }

            var otherSideProperty = this._otherSideProperty;
            if (otherSideProperty != null)
            {
                return (ICollection<T>) otherSideProperty.GetValue(otherSide, null);
            }

            return null;
        }
    }
}