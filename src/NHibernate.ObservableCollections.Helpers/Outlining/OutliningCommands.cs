namespace NHibernate.ObservableCollections.Helpers.Outlining
{
    #region Using Directives

    using System.Windows.Input;

    #endregion

    /// <summary>Defines outlining-related routed-commands</summary>
    public static class OutliningCommands
    {
        public static readonly RoutedUICommand MoveDemote =
            new RoutedUICommand("Demote", "MoveDemote", typeof(OutliningCommands));

        public static readonly RoutedUICommand MoveFrontward =
            new RoutedUICommand("Frontward", "MoveFrontward", typeof(OutliningCommands));

        public static readonly RoutedUICommand MovePromote =
            new RoutedUICommand("Promote", "MovePromote", typeof(OutliningCommands));

        public static readonly RoutedUICommand MoveRearward =
            new RoutedUICommand("Rearward", "MoveRearward", typeof(OutliningCommands));

        public static readonly RoutedUICommand NewChild =
            new RoutedUICommand("Child", "NewChild", typeof(OutliningCommands));

        public static readonly RoutedUICommand NewParent =
            new RoutedUICommand("Parent", "NewParent", typeof(OutliningCommands));

        public static readonly RoutedUICommand NewSiblingAfter =
            new RoutedUICommand("Sibling After", "NewSiblingAfter", typeof(OutliningCommands));

        public static readonly RoutedUICommand NewSiblingBefore =
            new RoutedUICommand("Sibling Before", "NewSiblingBefore", typeof(OutliningCommands));
    }
}