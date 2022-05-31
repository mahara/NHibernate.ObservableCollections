using System.Windows.Input;

namespace NHibernate.ObservableCollections.Helpers.Outlining
{
    public record struct RelativePosition<T>
    {
        /// <summary>The desired index when inserting a child.</summary>
        public int ChildIndex;

        /// <summary>The outlining insertion command to use.</summary>
        public ICommand Command;

        /// <summary>The children of the parent, relative to which the new item will be positioned.</summary>
        public IList<T> InsertRelativeTo;

        /// <summary>The parent to the new item.</summary>
        public T Parent;

        /// <summary>The name of the sub-items property.</summary>
        public string SubItemsPropertyName;

        public RelativePosition(ICommand command, T parent, IList<T> insertRelativeTo)
        {
            Command = command;
            SubItemsPropertyName = null;
            Parent = parent;
            InsertRelativeTo = insertRelativeTo;
            ChildIndex = -1;
        }

        public RelativePosition(ICommand command, string subItemsPropertyName, T parent, IList<T> insertRelativeTo)
        {
            Command = command;
            SubItemsPropertyName = subItemsPropertyName;
            Parent = parent;
            InsertRelativeTo = insertRelativeTo;
            ChildIndex = -1;
        }
    }
}
