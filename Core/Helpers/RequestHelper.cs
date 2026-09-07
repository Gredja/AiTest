using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Config;
using Core.Models;
using RestSharp;

namespace Core.Helpers;

public class RequestHelper
{
    public async Task<RestResponse> Get(
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

        AddDefaultHeaders(request);

        if (additionalParams != null)
            AddParams(request, additionalParams);

        return await client.ExecuteAsync(request);
    }

    public async Task<RestResponse<T>> Get<T>(
        string endpoint,
        Method method,
        List<RequestDictionaryModel> additionalParams = null)
    {
        var start = DateTime.Now;
        var client = InitializationRestClient(Endpoints.BaseUrl);
        var request = new RestRequest(endpoint, method)
        {
            RequestFormat = DataFormat.Json
        };

        AddDefaultHeaders(request);

        if (additionalParams != null)
            AddParams(request, additionalParams);

        return await client.ExecuteAsync<T>(request);
    }

    public static RestRequest AddDefaultHeaders(RestRequest request)
    {
        request.AddHeader("Content-Type", "application/json;charset=utf-8");
        request.AddHeader("X-Lang", "en_GB");

        return request;
    }

    public static RestRequest AddParams(RestRequest request, List<RequestDictionaryModel> additionalParams)
    {
        foreach (var param in additionalParams)
        {
            switch (param.Type)
            {
                case "Header":
                    request.AddHeader(param.Key, param.Value?.ToString());
                    break;
                case "Parameter":
                    request.AddParameter(param.Key, param.Value?.ToString());
                    break;
                case "UrlSegment":
                    request.AddUrlSegment(param.Key, param.Value!.ToString());
                    break;
            }
        }

        return request;
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
