namespace NHibernate.ObservableCollections.Helpers
{
    using System.Collections.Generic;

    public static class CollectionsUtil
    {
        public static bool ContainsAll<T>(ICollection<T> collection, ICollection<T> values)
        {
            foreach (var val in values)
            {
                if (!collection.Contains(val))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ContainsAny<T>(ICollection<T> collection, ICollection<T> values)
        {
            foreach (var val in values)
            {
                if (collection.Contains(val))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Compares the contents of two collections independent of item order.
        ///     [Source: http://patforna.blogspot.com/2006/12/comparing-collections-independent-of.html ]
        /// </summary>
        /// <typeparam name="T">The collections' item type</typeparam>
        /// <returns>True if both collections contain the same elements (in any order)</returns>
        public static bool Equals<T>(ICollection<T> one, ICollection<T> two)
        {
            if (one == null || two == null)
            {
                // special case
                return one == two;
            }

            if (ReferenceEquals(one, two))
            {
                // same objects
                return true;
            }

            if (one.Count != two.Count)
            {
                return false;
            }

            IList<T> listOne = new List<T>(one);
            IList<T> listTwo = new List<T>(two);

            // make sure that every object in one is also in two
            for (var i = 0; i < listOne.Count; i++)
            {
                var obj = listOne[i];
                if (!listTwo.Contains(obj))
                {
                    return false;
                }

                listTwo.Remove(obj);
            }

            return true;
        }
    }
}
