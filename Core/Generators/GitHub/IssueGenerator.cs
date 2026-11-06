using Core.Config;
using Core.Models.GitHub;

namespace Core.Generators.GitHub;

public static class IssueGenerator
{
    public static CreateIssueModelRequest CreateRandom(string titlePrefix) =>
        new() { Title = DataGenerator.RandomTitle(titlePrefix) };

    public static CreateIssueModelRequest CreateRandom(string titlePrefix, string bodyPrefix) =>
        new() { Title = DataGenerator.RandomTitle(titlePrefix), Body = DataGenerator.RandomBody(bodyPrefix) };

    public static CreateIssueModelRequest CreateWithTitle(string title) =>
        new() { Title = title };

    public static CreateIssueModelRequest WithTitleBodyLabels(
        string titlePrefix, string bodyPrefix, List<string> labels) =>
        new()
        {
            Title = DataGenerator.RandomTitle(titlePrefix),
            Body = DataGenerator.RandomBody(bodyPrefix),
            Labels = labels
        };

    public static UpdateIssueModelRequest PatchTitle(string title) =>
        new() { Title = title };

    public static UpdateIssueModelRequest Close() =>
        new() { State = GitHubEndpoints.StateClosed };

    public static UpdateIssueModelRequest Reopen() =>
        new() { State = GitHubEndpoints.StateOpen };

    public static UpdateIssueModelRequest WithState(string state) =>
        new() { State = state };

    public static UpdateIssueModelRequest UnrecognizedState() =>
        new() { State = GitHubEndpoints.StateInvalid };
}
