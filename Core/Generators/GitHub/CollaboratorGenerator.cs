using Core.Models.GitHub;

namespace Core.Generators.GitHub;

public static class CollaboratorGenerator
{
    public static CreateCollaboratorModelRequest Create(string permission) =>
        new() { Permission = permission };
}
