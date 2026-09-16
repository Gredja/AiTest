using Core.Models.Generic;
using Core.Attributes;

namespace Core.Models.GitHub;

public class TagModelResponse : IdModel<long>
{
    [RequiredField]
    public string Name { get; set; }

    [RequiredField]
    public BranchCommit Commit { get; set; }
}
