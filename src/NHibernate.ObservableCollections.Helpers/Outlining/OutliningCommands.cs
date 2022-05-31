using System.Windows.Input;

namespace NHibernate.ObservableCollections.Helpers.Outlining
{
    /// <summary>
    ///     Defines outlining-related routed-commands.
    /// </summary>
    public static class OutliningCommands
    {
        public static readonly RoutedUICommand MoveDemote =
            new("Demote", "MoveDemote", typeof(OutliningCommands));

        public static readonly RoutedUICommand MoveFrontward =
            new("Frontward", "MoveFrontward", typeof(OutliningCommands));

        public static readonly RoutedUICommand MovePromote =
            new("Promote", "MovePromote", typeof(OutliningCommands));

        public static readonly RoutedUICommand MoveRearward =
            new("Rearward", "MoveRearward", typeof(OutliningCommands));

        public static readonly RoutedUICommand NewChild =
            new("Child", "NewChild", typeof(OutliningCommands));

        public static readonly RoutedUICommand NewParent =
            new("Parent", "NewParent", typeof(OutliningCommands));

        public static readonly RoutedUICommand NewSiblingAfter =
            new("Sibling After", "NewSiblingAfter", typeof(OutliningCommands));

        public static readonly RoutedUICommand NewSiblingBefore =
            new("Sibling Before", "NewSiblingBefore", typeof(OutliningCommands));
    }
}
