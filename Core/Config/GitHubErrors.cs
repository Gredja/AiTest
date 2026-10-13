namespace Core.Config;

public static class GitHubErrors
{
    // Verified against live API — documented in GitHubObservableBehaviour.md
    public const string NotFound = "Not Found";
    public const string BranchNotFound = "Branch not found";
    public const string RequiresAuthentication = "Requires authentication";
    public const string BadCredentials = "Bad credentials";
    public const string MissingTitle = "Invalid request.\n\n\"title\" wasn't supplied.";
    public const string LabelsNull = "Invalid request.\n\nFor 'properties/labels', nil is not an array.";
}
