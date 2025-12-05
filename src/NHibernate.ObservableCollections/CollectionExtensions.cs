namespace Iesi.Collections.Generic
{
    public static class ListExtensions
    {
        /// <summary>
        ///     Creates a shallow copy of a range of elements in the source <see cref="List{T}" />.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The zero-based <see cref="List{T}" /> index at which the range starts.</param>
        /// <param name="range">
        ///     The number of elements in the range.
        ///     start is the inclusive start index of the range.
        ///     end is the exclusive end index of the range.
        /// </param>
        /// <returns>A shallow copy of a range of elements in the source <see cref="List{T}" />.</returns>
        /// <remarks>
        ///     REFERENCES:
        ///     -   <see href="https://stackoverflow.com/questions/67856174/how-to-use-ranges-with-list-in-c" />
        ///     -   <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.getrange" />
        ///     -   <see href="https://learn.microsoft.com/en-us/dotnet/api/system.range" />
        ///     -   <see href="https://learn.microsoft.com/en-us/dotnet/api/system.range.-ctor" />
        ///     -   <see href="https://github.com/dotnet/maintenance-packages/issues/243" />
        ///         -   <see href="https://github.com/dotnet/msbuild/issues/12780" />
        ///     -   <see href="https://github.com/dotnet/runtime/issues/121539" />
        ///     -   <see href="https://github.com/dotnet/sdk/issues/51265" />
        /// </remarks>
        public static List<T> GetRange<T>(this List<T> list, Range range)
        {
            var (index, count) = range.GetOffsetAndLength(list.Count);
            return list.GetRange(index, count);
        }
    }
}
