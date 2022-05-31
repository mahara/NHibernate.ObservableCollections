namespace NHibernate.ObservableCollections.Helpers.Outlining
{
    using System.Collections.Generic;
    using System.Windows.Input;

    public record struct RelativePosition<T>
    {
        /// <summary>Desired index when inserting a child.</summary>
        public int ChildIndex;

        /// <summary>The outlining insertion command to use.</summary>
        public ICommand Command;

        /// <summary>
        ///     Children of the parent,
        ///     relative to which the new item will be positioned.
        /// </summary>
        public IList<T> InsertRelativeTo;

        /// <summary>Parent to the new item.</summary>
        public T Parent;

        /// <summary>Name of the sub-items property.</summary>
        public string SubItemsPropName;

        public RelativePosition(ICommand command, T parent, IList<T> insertRelativeTo)
        {
            Command = command;
            SubItemsPropName = null;
            Parent = parent;
            InsertRelativeTo = insertRelativeTo;
            ChildIndex = -1;
        }

        public RelativePosition(ICommand command, string subItemsPropName, T parent, IList<T> insertRelativeTo)
        {
            Command = command;
            SubItemsPropName = subItemsPropName;
            Parent = parent;
            InsertRelativeTo = insertRelativeTo;
            ChildIndex = -1;
        }
    }
}
