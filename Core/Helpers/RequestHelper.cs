using Core.Config;
using Core.Models;
using RestSharp;

namespace Core.Helpers;

public class RequestHelper
{
    private static readonly RestClient FakeStoreClient = CreateClient(FakeStoreEndpoints.BaseUrl);
    private static readonly RestClient JsonPlaceholderClient = CreateClient(JsonPlaceholderEndpoints.BaseUrl);
    private static readonly RestClient GitHubClient = CreateClient(GitHubEndpoints.BaseUrl);
    private static readonly Lazy<string?> _githubToken = new(() => GitHubEndpoints.Token);

    protected RestClient Client { get; private set; } = FakeStoreClient;

    protected void UseJsonPlaceholder() => Client = JsonPlaceholderClient;

    protected void UseFakeStore() => Client = FakeStoreClient;

    protected void UseGitHub() => Client = GitHubClient;

    public async Task<RestResponse<T>> Get<T>(
        string endpoint,
        Method method,
        List<RequestDictionaryModel> additionalParams = null)
    {
        var request = new RestRequest(endpoint, method)
        {
            RequestFormat = DataFormat.Json
        };

        AddDefaultHeaders(request);
        AddGitHubAuth(request);

        if (additionalParams != null)
        {
            AddParams(request, additionalParams);
        }

        return await Client.ExecuteAsync<T>(request);
    }

    public async Task<RestResponse<TResponse>> Post<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        List<RequestDictionaryModel> additionalParams = null)
        where TRequest : class =>
        await Send<TRequest, TResponse>(endpoint, Method.Post, body, additionalParams);

    public async Task<RestResponse<TResponse>> Put<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        List<RequestDictionaryModel> additionalParams = null)
        where TRequest : class =>
        await Send<TRequest, TResponse>(endpoint, Method.Put, body, additionalParams);

    public async Task<RestResponse<TResponse>> Patch<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        List<RequestDictionaryModel> additionalParams = null)
        where TRequest : class =>
        await Send<TRequest, TResponse>(endpoint, Method.Patch, body, additionalParams);

    private async Task<RestResponse<TResponse>> Send<TRequest, TResponse>(
        string endpoint,
        Method method,
        TRequest body,
        List<RequestDictionaryModel> additionalParams = null)
        where TRequest : class
    {
        var request = new RestRequest(endpoint, method)
        {
            RequestFormat = DataFormat.Json
        };

        AddDefaultHeaders(request);
        AddGitHubAuth(request);
        request.AddJsonBody(body);

        if (additionalParams != null)
        {
            AddParams(request, additionalParams);
        }

        return await Client.ExecuteAsync<TResponse>(request);
    }

    public async Task<RestResponse<T>> Delete<T>(
        string endpoint,
        List<RequestDictionaryModel> additionalParams = null)
    {
        var request = new RestRequest(endpoint, Method.Delete)
        {
            RequestFormat = DataFormat.Json
        };

        AddDefaultHeaders(request);
        AddGitHubAuth(request);

        if (additionalParams != null)
        {
            AddParams(request, additionalParams);
        }

        return await Client.ExecuteAsync<T>(request);
    }

    private static RestRequest AddDefaultHeaders(RestRequest request)
    {
        request.AddHeader("Content-Type", "application/json;charset=utf-8");
        request.AddHeader("X-Lang", "en_GB");

        return request;
    }

    private void AddGitHubAuth(RestRequest request)
    {
        if (ReferenceEquals(Client, GitHubClient) && !string.IsNullOrEmpty(_githubToken.Value))
        {
            request.AddHeader("Authorization", $"Bearer {_githubToken.Value}");
        }
    }

    private static RestRequest AddParams(RestRequest request, List<RequestDictionaryModel> additionalParams)
    {
        foreach (var param in additionalParams)
        {
            switch (param.Type)
            {
                case ParamType.Header:
                    request.AddHeader(param.Key, param.Value?.ToString());
                    break;
                case ParamType.Parameter:
                    request.AddParameter(param.Key, param.Value?.ToString());
                    break;
                case ParamType.UrlSegment:
                    request.AddUrlSegment(param.Key, param.Value!.ToString());
                    break;
            }
        }

        return request;
    }

    private static RestClient CreateClient(string baseUrl)
    {
        var options = new RestClientOptions(baseUrl);

        if (TestConfig.SkipSslValidation)
        {
            options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        }

        return new RestClient(options);
    }
}
