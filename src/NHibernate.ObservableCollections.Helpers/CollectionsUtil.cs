namespace NHibernate.ObservableCollections.Helpers
{
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
        ///     [Source: https://patforna.blogspot.com/2006/12/comparing-collections-independent-of.html ]
        /// </summary>
        /// <typeparam name="T">The collections' item type</typeparam>
        /// <returns>True if both collections contain the same elements (in any order)</returns>
        public static bool Equals<T>(ICollection<T> collectionA, ICollection<T> collectionB)
        {
            if (collectionA is null || collectionB is null)
            {
                // special case
                return collectionA == collectionB;
            }

            if (ReferenceEquals(collectionA, collectionB))
            {
                // same objects
                return true;
            }

            if (collectionA.Count != collectionB.Count)
            {
                return false;
            }

            var listA = new List<T>(collectionA);
            var listB = new List<T>(collectionB);

            // make sure that every object in one is also in two
            for (var i = 0; i < listA.Count; i++)
            {
                var obj = listA[i];
                if (!listB.Contains(obj))
                {
                    return false;
                }

                listB.Remove(obj);
            }

            return true;
        }
    }
}
