namespace Core.Helpers;

public static class TestDataConstants
{
    public const int TitleRandomLength = 8;
    public const int BodyRandomLength = 16;
    public const int RandomNameLength = 8;
    public const int SingleItemPageSize = 1;
    public const int ExistingIssueNumber = 5;
    public const string NonNumericValue = "abc";
    public const string SpecialCharsSegment = "sp@ec!al";
    public const string ProbeTitle = "probe";
    public const string EntryCriteriaRepoDataMessage = "repo must keep data — Entry Criteria, documentation/GitHubTestingStructure.md";
    public const string EntryCriteriaPullRequestMessage = "repo must keep at least one PR — Entry Criteria, documentation/GitHubTestingStructure.md";
    public const string IssueContractCheckTitle = "Test issue for contract check";
    public const string IssueListNotEmptyMessage = "Setup should have guaranteed at least one issue";
    public const string PatchPersistTitleMessage = "PATCH must persist the new title";
    public const string ClosedStatePersistMessage = "closed state must persist";
    public const string UnrecognizedStateIgnoredMessage = "unrecognized state must be ignored — OB §19";
    public const string MergeResponseHasBodyMessage = "200 merge response must carry a body";
    public const string PullRequestEchoTitleMessage = "PR must echo the requested title";
    public const string MergeReportsMergedTrueMessage = "merge endpoint must report merged=true for a mergeable pull request";
    public const string TopicNameLowercaseMessage = "topic names must be lowercase alphanumeric with hyphens";
    public const string RateLimitRemainingHeaderMessage = "counting response must carry the remaining quota header";
}
