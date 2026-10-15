using Core.Attributes;

namespace Core.Models.GitHub;

public class CreateCollaboratorModelRequest
{
    [RequiredField]
    public string Permission { get; set; }
}
