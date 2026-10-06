using Core.Config;
using Core.Models.Generic;
using RestSharp;

namespace Core.Helpers.GitHub;

public abstract class GitHubTestBase : GitHubRequestHelper
{
    protected const int FirstPage = 1;
    protected const int DefaultPageSize = 5;
    protected const int SmallPageSize = 2;
    protected const string InvalidToken = "ghp_invalidtoken123";

    protected static List<RequestDictionaryModel> TestRepoParam()
    {
        var (owner, repo) = ParseRepo();

        return GitHubParamHelper.RepoParam(owner, repo);
    }

    protected static (string Owner, string Repo) ParseRepo() => ParseRepo(TestConfig.GitHubTestRepo);

    protected async Task<RestResponse> ExecuteWithAuthorization(
        string endpoint, string? authorization, List<RequestDictionaryModel>? additionalParams = null)
    {
        var request = new RestRequest(endpoint, Method.Get) { RequestFormat = DataFormat.Json };

        if (authorization is not null)
        {
            request.AddHeader("Authorization", authorization);
        }

        if (additionalParams is not null)
        {
            AddParams(request, additionalParams);
        }

        return await Client.ExecuteAsync(request);
    }

    private static (string Owner, string Repo) ParseRepo(string fullName)
    {
        var parts = fullName.Split('/');

        return (parts[0], parts[1]);
    }
}
