using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using RestSharp;

namespace Core.Helpers;

public class RequestHelper
{
    protected async Task<RestResponse> Get(
        string url,
        string endpoint,
        Method method,
        List<string> multiFilesPath = null,
        List<RequestDictionaryModel> additionalParams = null)
    {
        var start = DateTime.Now;
        var client = InitializationRestClient(url);
        var request = new RestRequest(endpoint, method)
        {
            RequestFormat = DataFormat.Json
        };

        // TODO: implement Get method body
        return null;
    }

    private static RestClient InitializationRestClient(string url)
    {
        var options = new RestClientOptions(url)
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };

        return new RestClient(options);
    }
}
