namespace Core.Models.GitHub;

public class RateLimitModelResponse
{
    public RateLimitResources Resources { get; set; }
    public RateLimitSection Rate { get; set; }
}
