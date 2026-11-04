using Core.Generators;
using Core.Helpers.GitHub;
using System.Security.Cryptography;
using System.Text;
using static Core.Helpers.TestDataConstants;

namespace Api.GitHub.Helpers;

public abstract class GitBlobTestBase : GitHubTestBase
{
    protected static string RandomBlobContent() =>
        $"blob-{DataGenerator.RandomString(RandomNameLength)}";

    // Git blob object format: sha1("blob <byte length>\0" + content) — see git-scm docs, git-hash-object
    protected static string ExpectedBlobSha(string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var header = Encoding.ASCII.GetBytes($"blob {contentBytes.Length}\0");
        var payload = header.Concat(contentBytes).ToArray();

        using var sha1 = SHA1.Create();

        return Convert.ToHexString(sha1.ComputeHash(payload)).ToLowerInvariant();
    }
}
