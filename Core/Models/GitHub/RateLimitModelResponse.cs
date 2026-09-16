namespace Core.Models.GitHub;

public class RateLimitModelResponse
{
    public RateLimitResourcesModel Resources { get; set; }
    public RateLimitSectionModel Rate { get; set; }
}
