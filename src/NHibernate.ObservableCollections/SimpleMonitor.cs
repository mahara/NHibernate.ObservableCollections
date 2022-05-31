namespace Iesi.Collections.Generic
{
    using System;

    /// <summary>
    ///     A simple monitor that helps preventing reentrant calls.
    /// </summary>
    /// <author>Microsoft Corporation</author>
    /// <remarks>
    ///     <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.ObjectModel/src/System/Collections/ObjectModel/ObservableCollection.cs" />
    ///     <see href="https://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
    /// </remarks>
    /// <inheritdoc />
    internal class SimpleMonitor : IDisposable
    {
        private int _busyCount;

        public bool Busy =>
            _busyCount > 0;

        public void Dispose()
        {
            _busyCount--;
        }

        public void Enter()
        {
            _busyCount++;
        }
    }
}
