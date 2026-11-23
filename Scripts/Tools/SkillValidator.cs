using System.Text;
using System.Text.RegularExpressions;

namespace Gredja.Tools;

// ERROR:/WARNING:/PASS/FAIL lines are a parse contract: the /commit structure gate and
// skill-creator parity runs consume them verbatim — message wording must stay stable.
internal static class SkillValidator
{
    private const int MaxDescriptionLength = 1024;
    private const int MaxBodyWords = 5000;

    private static readonly Regex _kebabRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly Regex _frontmatterRegex = new(@"^---\s*\n(.*?)\n---\s*\n?", RegexOptions.Singleline);
    private static readonly Regex _referenceRegex = new(@"(?:scripts|references|assets)/[\w./-]*\w");
    private static readonly string[] _reservedWords = ["claude", "anthropic"];
    private static readonly string[] _whenCues = ["use when", "use this", "use for", "trigger", "use it when"];

    private static readonly List<string> _errors = [];
    private static readonly List<string> _warnings = [];

    internal static int Run(string skillDirectoryArgument)
    {
        _errors.Clear();
        _warnings.Clear();

        var skillDirectory = Path.GetFullPath(skillDirectoryArgument);
        if (!Directory.Exists(skillDirectory))
        {
            Console.WriteLine($"ERROR: not a directory: {skillDirectory}");
            return 2;
        }

        var folder = new DirectoryInfo(skillDirectory).Name;
        var entries = Directory.GetFileSystemEntries(skillDirectory)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Select(name => name!)
            .ToList();

        if (!_kebabRegex.IsMatch(folder))
        {
            _errors.Add($"folder name {Repr(folder)} is not kebab-case (lowercase, digits, hyphens only)");
        }

        if (!entries.Contains("SKILL.md"))
        {
            var near = entries.FirstOrDefault(entry => entry.Equals("skill.md", StringComparison.OrdinalIgnoreCase));
            if (near is not null)
            {
                _errors.Add($"found {Repr(near)} — must be named exactly 'SKILL.md' (case-sensitive)");
            }
            else
            {
                _errors.Add("SKILL.md is missing");
            }

            Report();
            return 1;
        }

        if (entries.Any(entry => entry.Equals("readme.md", StringComparison.OrdinalIgnoreCase)))
        {
            _errors.Add("README.md must not be inside the skill folder (put docs in SKILL.md or references/)");
        }

        var text = ReadNormalized(Path.Combine(skillDirectory, "SKILL.md"));

        var (fields, body) = ParseFrontmatter(text);
        if (fields is null)
        {
            _errors.Add("frontmatter missing or malformed: SKILL.md must start with '---' delimited YAML");
            Report();
            return 1;
        }

        var frontmatterBlock = ExtractFrontmatterBlock(text);
        if (frontmatterBlock.Contains('<') || frontmatterBlock.Contains('>'))
        {
            _errors.Add("frontmatter contains XML angle brackets (< >) — forbidden for security");
        }

        ValidateName(fields, folder);
        ValidateDescription(fields);
        ValidateCompatibility(fields);

        var wordCount = body.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount > MaxBodyWords)
        {
            _warnings.Add($"SKILL.md body is {wordCount} words (recommended max {MaxBodyWords}) — move detail to references/");
        }

        ValidateReferencedPaths(skillDirectory, body);

        Report();
        return _errors.Count > 0 ? 1 : 0;
    }

    private static void ValidateName(IReadOnlyDictionary<string, string> fields, string folder)
    {
        var name = fields.GetValueOrDefault("name") ?? string.Empty;
        if (name.Length == 0)
        {
            _errors.Add("frontmatter is missing required field 'name'");
            return;
        }

        if (!_kebabRegex.IsMatch(name))
        {
            _errors.Add($"name {Repr(name)} is not kebab-case");
        }

        if (name != folder)
        {
            _warnings.Add($"name {Repr(name)} does not match folder name {Repr(folder)}");
        }

        var loweredName = name.ToLowerInvariant();
        foreach (var reservedWord in _reservedWords.Where(loweredName.Contains))
        {
            _errors.Add($"name {Repr(name)} uses a reserved word ({string.Join('/', _reservedWords)})");
            break;
        }
    }

    private static void ValidateDescription(IReadOnlyDictionary<string, string> fields)
    {
        var description = fields.GetValueOrDefault("description") ?? string.Empty;
        if (description.Length == 0)
        {
            _errors.Add("frontmatter is missing required field 'description'");
            return;
        }

        if (description.Length > MaxDescriptionLength)
        {
            _errors.Add($"description is {description.Length} chars (max {MaxDescriptionLength})");
        }

        if (description.Length < 40)
        {
            _warnings.Add($"description is very short ({description.Length} chars) — likely too vague to trigger");
        }

        var loweredDescription = description.ToLowerInvariant();
        if (!_whenCues.Any(loweredDescription.Contains))
        {
            _warnings.Add("description has no obvious WHEN clause (e.g. 'Use when ...') — add trigger conditions");
        }
    }

    private static void ValidateCompatibility(IReadOnlyDictionary<string, string> fields)
    {
        var compatibility = fields.GetValueOrDefault("compatibility") ?? string.Empty;
        if (compatibility.Length > 0 && (compatibility.Length < 1 || compatibility.Length > 500))
        {
            _errors.Add($"compatibility must be 1-500 chars (got {compatibility.Length})");
        }
    }

    private static void ValidateReferencedPaths(string skillDirectory, string body)
    {
        foreach (Match match in _referenceRegex.Matches(body))
        {
            var relativePath = match.Value;
            var combinedPath = Path.Combine(skillDirectory, relativePath);
            if (!File.Exists(combinedPath) && !Directory.Exists(combinedPath))
            {
                _warnings.Add($"SKILL.md references {Repr(relativePath)} but it does not exist in the skill folder");
            }
        }
    }

    private static (Dictionary<string, string>? Fields, string Body) ParseFrontmatter(string text)
    {
        if (!text.StartsWith("---", StringComparison.Ordinal))
        {
            return (null, text);
        }

        var match = _frontmatterRegex.Match(text);
        if (!match.Success)
        {
            return (null, text);
        }

        var fields = new Dictionary<string, string>();
        string? currentKey = null;
        foreach (var line in match.Groups[1].Value.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith(' ') || line.StartsWith('\t'))
            {
                if (currentKey is not null)
                {
                    fields[currentKey] += " " + line.Trim();
                }

                continue;
            }

            var colonIndex = line.IndexOf(':');
            if (colonIndex < 0)
            {
                _warnings.Add($"frontmatter line has no key: {Repr(line)}");
                continue;
            }

            currentKey = line[..colonIndex].Trim();
            fields[currentKey] = line[(colonIndex + 1)..].Trim().Trim('"', '\'');
        }

        return (fields, text[(match.Index + match.Length)..]);
    }

    private static string ExtractFrontmatterBlock(string text)
    {
        var firstDelimiter = text.IndexOf("---", StringComparison.Ordinal);
        if (firstDelimiter < 0)
        {
            return string.Empty;
        }

        var secondDelimiter = text.IndexOf("---", firstDelimiter + 3, StringComparison.Ordinal);
        if (secondDelimiter < 0)
        {
            return string.Empty;
        }

        return text[(firstDelimiter + 3)..secondDelimiter];
    }

    private static string ReadNormalized(string path)
    {
        // Python reads text files with universal-newline translation; mirror it so line
        // splitting and parity diffs match the vendored script on CRLF worktrees.
        using var reader = new StreamReader(path, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: false);
        return reader.ReadToEnd().Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private static string Repr(string value)
    {
        var useDoubleQuotes = value.Contains('\'') && !value.Contains('"');
        var escaped = useDoubleQuotes
            ? value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)
            : value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal);
        escaped = escaped.Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);

        return useDoubleQuotes ? $"\"{escaped}\"" : $"'{escaped}'";
    }

    private static void Report()
    {
        foreach (var message in _errors)
        {
            Console.WriteLine($"ERROR: {message}");
        }

        foreach (var message in _warnings)
        {
            Console.WriteLine($"WARNING: {message}");
        }

        var verdict = _errors.Count > 0 ? "FAIL" : "PASS";
        Console.WriteLine($"{verdict}: {_errors.Count} error(s), {_warnings.Count} warning(s)");
    }
}
