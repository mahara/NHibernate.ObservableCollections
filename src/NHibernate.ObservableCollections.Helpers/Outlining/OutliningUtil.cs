namespace NHibernate.ObservableCollections.Helpers.Outlining
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides utility methods for manipulating tree structures.
    /// </summary>
    public static class OutliningUtil
    {
        public static string GenerateUniqueName<T>(string newItemType, string nameProperty, ICollection<T> parentCollection)
        {
            string result = null;
            var isNameUnique = false;

            for (var i = 0; !isNameUnique; i++)
            {
                //find a unique name to use
                result = i == 0 ? "New " + newItemType : "New " + newItemType + " (" + i + ")";
                isNameUnique = true;
                foreach (var existingItem in parentCollection)
                {
                    if (result.Equals(ReflectionUtil.NavigateToOneSide(existingItem, nameProperty)))
                    {
                        isNameUnique = false;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Inserts a new node into a tree structure.
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="relativePos"></param>
        public static void Insert<T>(T newItem, RelativePosition<T> relativePos)
        {
            var subItems =
                (IList<T>) ReflectionUtil.NavigateToManySide<T>(relativePos.Parent, relativePos.SubItemsPropName);
            var newIndex = -1;

            if (relativePos.Command == OutliningCommands.NewSiblingBefore)
            {
                newIndex = MinimumIndex(subItems, relativePos.InsertRelativeTo);
            }
            else if (relativePos.Command == OutliningCommands.NewSiblingAfter)
            {
                newIndex = MaximumIndex(subItems, relativePos.InsertRelativeTo) + 1;
            }

            //((Topic)parent).SubTopics.Insert( newIndex, (Topic)newItem );
            else if (relativePos.Command == OutliningCommands.NewChild)
            {
                if (relativePos.ChildIndex >= 0)
                {
                    newIndex = relativePos.ChildIndex;
                }
                else
                {
                    newIndex = subItems.Count; // Insert the new item at the end of the sub-items list
                }
            }
            else if (relativePos.Command == OutliningCommands.NewParent)
            {
                //Insert the new item at the same position where the first selected item was located:
                newIndex = MinimumIndex(subItems, relativePos.InsertRelativeTo);
                var newSubItems =
                    (IList<T>) ReflectionUtil.NavigateToManySide<T>(newItem, relativePos.SubItemsPropName);
                foreach (var item in relativePos.InsertRelativeTo)
                {
                    //loop thru selected items:
                    subItems.Remove(item); //remove item from parent's sub-topics list
                    newSubItems.Add(item); //add it as child of the new item
                }
            }

            subItems.Insert(newIndex, newItem);
        }

        private static int MaximumIndex<T>(IList<T> parentList, IList<T> subItems)
        {
            var result = 0;
            foreach (var item in subItems)
            {
                var index = parentList.IndexOf(item);
                if (index > result)
                {
                    result = index;
                }
            }

            return result;
        }

        private static int MinimumIndex<T>(IList<T> parentList, IList<T> subItems)
        {
            var result = parentList.Count;
            foreach (var item in subItems)
            {
                var index = parentList.IndexOf(item);
                if (index < result)
                {
                    result = index;
                }
            }

            return result;
        }
    }
}
