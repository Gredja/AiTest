namespace Core.Models.GitHub;

public class CollaboratorPermissions
{
    public bool Admin { get; set; }

    public bool Maintain { get; set; }

    public bool Push { get; set; }

    public bool Triage { get; set; }

    public bool Pull { get; set; }
}
