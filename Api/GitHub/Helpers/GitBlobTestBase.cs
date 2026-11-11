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
        var payload = new byte[header.Length + contentBytes.Length];
        Buffer.BlockCopy(header, 0, payload, 0, header.Length);
        Buffer.BlockCopy(contentBytes, 0, payload, header.Length, contentBytes.Length);

        using var sha1 = SHA1.Create();

        return Convert.ToHexString(sha1.ComputeHash(payload)).ToLowerInvariant();
    }
}
