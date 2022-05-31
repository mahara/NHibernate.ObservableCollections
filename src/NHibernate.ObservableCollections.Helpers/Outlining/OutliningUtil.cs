namespace NHibernate.ObservableCollections.Helpers.Outlining
{
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
                // Find a unique name to use
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
        /// <param name="relativePosition"></param>
        public static void Insert<T>(T newItem, RelativePosition<T> relativePosition)
        {
            var subItems = (IList<T>) ReflectionUtil.NavigateToManySide<T>(relativePosition.Parent, relativePosition.SubItemsPropertyName);
            var newIndex = -1;

            if (relativePosition.Command == OutliningCommands.NewSiblingBefore)
            {
                newIndex = MinimumIndex(subItems, relativePosition.InsertRelativeTo);
            }
            else if (relativePosition.Command == OutliningCommands.NewSiblingAfter)
            {
                newIndex = MaximumIndex(subItems, relativePosition.InsertRelativeTo) + 1;
            }

            //((Topic) parent).SubTopics.Insert(newIndex, (Topic) newItem);
            else if (relativePosition.Command == OutliningCommands.NewChild)
            {
                if (relativePosition.ChildIndex >= 0)
                {
                    newIndex = relativePosition.ChildIndex;
                }
                else
                {
                    newIndex = subItems.Count; // Insert the new item at the end of the sub-items list
                }
            }
            else if (relativePosition.Command == OutliningCommands.NewParent)
            {
                // Insert the new item at the same position where the first selected item was located
                newIndex = MinimumIndex(subItems, relativePosition.InsertRelativeTo);
                var newSubItems = (IList<T>) ReflectionUtil.NavigateToManySide<T>(newItem, relativePosition.SubItemsPropertyName);
                foreach (var item in relativePosition.InsertRelativeTo)
                {
                    // Loop thru selected items
                    subItems.Remove(item); // Remove item from parent's sub-topics list
                    newSubItems.Add(item); // Add it as child of the new item
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
