using RestSharp;

namespace Core.Helpers;

public static class WaitDiagnosticsHelper
{
    public static string Describe<T>(WaitResult<T> result)
        where T : RestResponse =>
        $"waited {result.Elapsed}, attempts {result.Attempts}, last status {result.LastValue?.StatusCode}; informational on live services";
}
