namespace Iesi.Collections.Generic
{
    #region Using Directives

    using System;

    #endregion

    /// <summary>
    ///     A simple monitor that helps preventing reentrant calls.
    /// </summary>
    /// <author>Microsoft Corporation</author>
    /// <remarks>
    ///     <see href="http://referencesource.microsoft.com/#System/compmod/system/collections/objectmodel/observablecollection.cs" />
    /// </remarks>
    /// <inheritdoc />
    internal class SimpleMonitor : IDisposable
    {
        private int _busyCount;

        public bool Busy => this._busyCount > 0;

        public void Dispose()
        {
            this._busyCount--;
        }

        public void Enter()
        {
            this._busyCount++;
        }
    }
}