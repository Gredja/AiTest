using Core.Models.GitHub;
using static Core.Helpers.TestDataConstants;

namespace Core.Generators.GitHub;

public static class ReleaseGenerator
{
    public static CreateReleaseModelRequest CreateRandom(string tagPrefix, string name) =>
        new()
        {
            TagName = $"{tagPrefix}{DataGenerator.RandomString(RandomNameLength)}",
            Name = name
        };
}
