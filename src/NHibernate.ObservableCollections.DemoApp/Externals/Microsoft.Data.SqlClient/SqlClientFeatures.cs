#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Data.SqlClient;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class SqlClientFeatures
{
    /// <summary>
    ///     Enables or disables the new asynchronous behavior for the SQL client.
    /// </summary>
    /// <param name="enable">
    ///     If set to <c>true</c>, enables the new asynchronous behavior;
    ///     otherwise, disables it.
    /// </param>
    /// <remarks>
    ///     REFERENCES:
    ///     -   <see href="https://github.com/dotnet/SqlClient/blob/main/release-notes/7.0/7.0.0.md" />
    /// </remarks>
    public static void EnableNewAsyncBehavior(bool enable = true)
    {
        if (enable)
        {
            AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseCompatibilityAsyncBehaviour", false);
            AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseCompatibilityProcessSni", false);
        }
        else
        {
            AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseCompatibilityAsyncBehaviour", true);
            AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseCompatibilityProcessSni", true);
        }
    }
}
