using Core.Attributes;

namespace Core.Models.JsonPlaceholder;

public class CompanyModel
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public string CatchPhrase { get; set; }

    [RequiredField]
    public string Bs { get; set; }
}
