using Core.Config;
using Core.Models.GitHub;

namespace Core.Generators.GitHub;

public static class IssueGenerator
{
    private const string UnrecognizedStateValue = "bogus";

    public static CreateIssueModelRequest CreateRandom(string titlePrefix) =>
        new() { Title = DataGenerator.RandomTitle(titlePrefix) };

    public static CreateIssueModelRequest CreateRandom(string titlePrefix, string bodyPrefix) =>
        new() { Title = DataGenerator.RandomTitle(titlePrefix), Body = DataGenerator.RandomBody(bodyPrefix) };

    public static CreateIssueModelRequest CreateWithTitle(string title) =>
        new() { Title = title };

    public static UpdateIssueModelRequest PatchTitle(string title) =>
        new() { Title = title };

    public static UpdateIssueModelRequest Close() =>
        new() { State = GitHubEndpoints.StateClosed };

    public static UpdateIssueModelRequest Reopen() =>
        new() { State = GitHubEndpoints.StateOpen };

    public static UpdateIssueModelRequest WithState(string state) =>
        new() { State = state };

    public static UpdateIssueModelRequest UnrecognizedState() =>
        new() { State = UnrecognizedStateValue };
}
