namespace Iesi.Collections.Generic
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///     REFERENCES:
    ///     -   https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/ObjectModel/ReadOnlyCollection.cs
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
    public class ReadOnlyList<T> : ReadOnlyCollection<T>
    {
        public ReadOnlyList(IList<T> list) : base(list)
        {
        }
    }
}
