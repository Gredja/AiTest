namespace Core.Models.GitHub;

public class RateLimitSection
{
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public long Reset { get; set; }
    public int Used { get; set; }
}
