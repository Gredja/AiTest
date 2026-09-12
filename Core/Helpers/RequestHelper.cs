using Core.Config;
using Core.Models;
using RestSharp;

namespace Core.Helpers;

public class RequestHelper
{
    private static readonly RestClient _fakeStoreClient = CreateClient(FakeStoreEndpoints.BaseUrl);
    private static readonly RestClient _jsonPlaceholderClient = CreateClient(JsonPlaceholderEndpoints.BaseUrl);
    private static readonly RestClient _gitHubClient = CreateClient(GitHubEndpoints.BaseUrl);
    private static readonly Lazy<string?> _githubToken = new(() => GitHubEndpoints.Token);

    protected RestClient Client { get; private set; } = _fakeStoreClient;

    protected void UseJsonPlaceholder() => Client = _jsonPlaceholderClient;

    protected void UseFakeStore() => Client = _fakeStoreClient;

    protected void UseGitHub() => Client = _gitHubClient;

    public async Task<RestResponse<T>> Get<T>(
        string endpoint,
        Method method,
        List<RequestDictionaryModel>? additionalParams = null)
    {
        var request = new RestRequest(endpoint, method)
        {
            RequestFormat = DataFormat.Json
        };

        AddDefaultHeaders(request);
        AddGitHubAuth(request);

        if (additionalParams is not null)
        {
            AddParams(request, additionalParams);
        }

        return await Client.ExecuteAsync<T>(request);
    }

    public async Task<RestResponse<TResponse>> Post<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        List<RequestDictionaryModel>? additionalParams = null)
        where TRequest : class =>
        await Send<TRequest, TResponse>(endpoint, Method.Post, body, additionalParams);

    public async Task<RestResponse<TResponse>> Put<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        List<RequestDictionaryModel>? additionalParams = null)
        where TRequest : class =>
        await Send<TRequest, TResponse>(endpoint, Method.Put, body, additionalParams);

    public async Task<RestResponse<TResponse>> Patch<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        List<RequestDictionaryModel>? additionalParams = null)
        where TRequest : class =>
        await Send<TRequest, TResponse>(endpoint, Method.Patch, body, additionalParams);

    private async Task<RestResponse<TResponse>> Send<TRequest, TResponse>(
        string endpoint,
        Method method,
        TRequest body,
        List<RequestDictionaryModel>? additionalParams = null)
        where TRequest : class
    {
        var request = new RestRequest(endpoint, method)
        {
            RequestFormat = DataFormat.Json
        };

        AddDefaultHeaders(request);
        AddGitHubAuth(request);
        request.AddJsonBody(body);

        if (additionalParams is not null)
        {
            AddParams(request, additionalParams);
        }

        return await Client.ExecuteAsync<TResponse>(request);
    }

    public async Task<RestResponse<T>> Delete<T>(
        string endpoint,
        List<RequestDictionaryModel>? additionalParams = null)
    {
        var request = new RestRequest(endpoint, Method.Delete)
        {
            RequestFormat = DataFormat.Json
        };

        AddDefaultHeaders(request);
        AddGitHubAuth(request);

        if (additionalParams is not null)
        {
            AddParams(request, additionalParams);
        }

        return await Client.ExecuteAsync<T>(request);
    }

    private static void AddDefaultHeaders(RestRequest request)
    {
        request.AddHeader("Content-Type", "application/json;charset=utf-8");
        request.AddHeader("X-Lang", "en_GB");
    }

    private void AddGitHubAuth(RestRequest request)
    {
        if (!ReferenceEquals(Client, _gitHubClient) || string.IsNullOrEmpty(_githubToken.Value))
        {
            return;
        }

        request.AddHeader("Authorization", $"Bearer {_githubToken.Value}");
    }

    private static void AddParams(RestRequest request, List<RequestDictionaryModel> additionalParams)
    {
        foreach (var param in additionalParams)
        {
            switch (param.Type)
            {
                case ParamType.Header:
                    request.AddHeader(param.Key, param.Value?.ToString() ?? string.Empty);
                    break;
                case ParamType.Parameter:
                    request.AddParameter(param.Key, param.Value?.ToString());
                    break;
                case ParamType.UrlSegment:
                    request.AddUrlSegment(param.Key, param.Value?.ToString());
                    break;
            }
        }
    }

    private static RestClient CreateClient(string baseUrl)
    {
        var options = new RestClientOptions(baseUrl);

        if (TestConfig.IsSslValidationSkipped)
        {
            options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        }

        return new RestClient(options);
    }
}
