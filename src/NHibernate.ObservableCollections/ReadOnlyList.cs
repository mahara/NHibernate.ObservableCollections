using System.Diagnostics;

namespace Iesi.Collections.Generic
{
    /// <summary>
    ///     Provides the base class for a generic read-only list.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of elements in the list.
    /// </typeparam>
    /// <remarks>
    ///     REFERENCES:
    ///     -   <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.readonlycollection-1" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/ObjectModel/ReadOnlyCollection.cs" />
    ///     -   <see href="https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/Collections/ObjectModel/ReadOnlyCollection.cs" />
    /// </remarks>
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay($"{nameof(Count)} = {{{nameof(Count)}}}")]
    public class ReadOnlyList<T> :
        ReadOnlyCollection<T>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadOnlyList{T}" /> class
        ///     that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        public ReadOnlyList(IList<T> list) :
            base(list)
        {
        }
    }
}
