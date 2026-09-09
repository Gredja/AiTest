namespace Core.Models.GitHub;

public class CreateIssueRequest
{
    public string Title { get; set; }
    public string Body { get; set; }
    public List<string> Labels { get; set; }
}
