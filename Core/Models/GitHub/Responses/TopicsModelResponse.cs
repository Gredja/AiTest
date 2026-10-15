using Core.Attributes;

namespace Core.Models.GitHub;

public class TopicsModelResponse
{
    [RequiredField]
    public List<string> Names { get; set; }
}
