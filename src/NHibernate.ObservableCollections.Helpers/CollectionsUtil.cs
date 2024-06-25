namespace NHibernate.ObservableCollections.Helpers;

public static class CollectionsUtil
{
    public static bool ContainsAll<T>(ICollection<T> source, ICollection<T> target)
    {
        return source.All(target.Contains);
    }

    public static bool ContainsAny<T>(ICollection<T> source, ICollection<T> target)
    {
        return source.Any(target.Contains);
    }

    /// <summary>
    ///     Compares the contents of two collections independent of item order.
    /// </summary>
    /// <typeparam name="T">The collections' item type.</typeparam>
    /// <returns><see langword="true" /> if both collections contain the same elements (in any order).</returns>
    /// <remarks>
    ///     REFERENCES:
    ///     -   <see href="https://patforna.blogspot.com/2006/12/comparing-collections-independent-of.html" />
    /// </remarks>
    public static bool Equals<T>(ICollection<T> collectionA, ICollection<T> collectionB)
    {
        if (collectionA is null || collectionB is null)
        {
            // Special case
            return collectionA == collectionB;
        }

        if (ReferenceEquals(collectionA, collectionB))
        {
            // Same objects
            return true;
        }

        if (collectionA.Count != collectionB.Count)
        {
            return false;
        }

        var listA = new List<T>(collectionA);
        var listB = new List<T>(collectionB);

        // Make sure that every object in one is also in two
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
