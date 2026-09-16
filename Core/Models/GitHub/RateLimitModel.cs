namespace Core.Models.GitHub;

public class RateLimitModel
{
    public RateLimitResources Resources { get; set; }
    public RateLimitSection Rate { get; set; }
}
