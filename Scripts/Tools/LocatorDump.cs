using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Playwright;

namespace Gredja.Tools;

// One-shot accessibility-tree dump for UI-locator authoring (Rules/ui-testing.md → Locators).
// Behaviour mirrors the retired locator-dump.mjs: same CLI flags, same stdout line format,
// same stderr progress lines — parity diffs against the node version are the acceptance test.
internal static class LocatorDump
{
    private const int DefaultTimeoutMs = 15000;
    private const int PollIntervalMs = 100;
    private const int PostClickSettleMs = 400;
    private const int ViewportWidth = 1400;
    private const int ViewportHeight = 900;

    private static readonly string DefaultStatePath = Path.Combine(".auth", "github-ui-state.json");

    private static readonly HashSet<string> _interestingRoles =
    [
        "button", "link", "textbox", "searchbox", "combobox", "checkbox", "radio",
        "switch", "tab", "menuitem", "menuitemcheckbox", "menuitemradio", "option",
        "heading", "slider", "spinbutton", "treeitem", "summary", "alert", "status",
        "image"
    ];

    private static readonly HashSet<string> _clickableRoles =
    [
        "button", "link", "menuitem", "menuitemcheckbox", "menuitemradio",
        "checkbox", "tab", "summary", "combobox", "option"
    ];

    private static readonly JsonSerializerOptions _jsonStringOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly System.Text.RegularExpressions.Regex _whitespaceRegex = new(@"\s+");

    private static readonly string[] _platformDirectories = ["chrome-win64", "chrome-win", "chrome-linux"];

    internal static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var options = ParseArguments(args);
            if (options.ShowHelp || options.Url is null)
            {
                Console.WriteLine(Usage());
                return options.ShowHelp ? 0 : 1;
            }

            return await DumpAsync(options);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private sealed class Options
    {
        internal string? Url { get; set; }
        internal string? State { get; set; }
        internal bool IsAnonymous { get; set; }
        internal string? WaitFor { get; set; }
        internal string? WaitForAx { get; set; }
        internal List<string> Clicks { get; } = [];
        internal bool IncludeStaticText { get; set; }
        internal int TimeoutMs { get; set; } = DefaultTimeoutMs;
        internal bool IsHeaded { get; set; }
        internal bool ShowHelp { get; set; }
    }

    private static Options ParseArguments(string[] argumentList)
    {
        var options = new Options();
        for (var index = 0; index < argumentList.Length; index++)
        {
            var argument = argumentList[index];
            if (argument is "-h" or "--help")
            {
                options.ShowHelp = true;
            }
            else if (argument == "--state")
            {
                options.State = NextValue(argumentList, ref index, "--state requires a path");
            }
            else if (argument == "--anonymous")
            {
                options.IsAnonymous = true;
            }
            else if (argument == "--waitfor")
            {
                options.WaitFor = NextValue(argumentList, ref index, "--waitfor requires a text");
            }
            else if (argument == "--waitfor-ax")
            {
                options.WaitForAx = NextValue(argumentList, ref index, "--waitfor-ax requires a text");
            }
            else if (argument == "--click")
            {
                options.Clicks.Add(NextValue(argumentList, ref index, "--click requires a text"));
            }
            else if (argument == "--static-text")
            {
                options.IncludeStaticText = true;
            }
            else if (argument == "--timeout")
            {
                var rawTimeout = NextValue(argumentList, ref index, "--timeout requires a positive number of ms");
                if (!int.TryParse(rawTimeout, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timeoutMs)
                    || timeoutMs <= 0)
                {
                    throw new LocatorDumpException("--timeout requires a positive number of ms");
                }

                options.TimeoutMs = timeoutMs;
            }
            else if (argument == "--headed")
            {
                options.IsHeaded = true;
            }
            else if (argument.StartsWith("--", StringComparison.Ordinal))
            {
                throw new LocatorDumpException($"unknown option: {argument}");
            }
            else if (options.Url is not null)
            {
                throw new LocatorDumpException($"unexpected argument: {argument}");
            }
            else
            {
                options.Url = argument;
            }
        }

        return options;
    }

    private static string NextValue(string[] argumentList, ref int index, string error)
    {
        index++;
        if (index >= argumentList.Length)
        {
            throw new LocatorDumpException(error);
        }

        return argumentList[index];
    }

    private static string Usage() =>
        $"""
        Usage:
          dotnet run --project Scripts/Tools -- locator-dump <url> [options]

        Dumps role + accessible name of interactive elements (for GetByRole/GetByLabel locators).

        Options:
          --state <path>   storageState JSON with cookies
                           (default: {DefaultStatePath} when it exists)
          --anonymous      do not load any state
          --waitfor <text> wait until body innerText contains <text> (case-insensitive)
          --waitfor-ax <text>
                           wait until the accessibility tree contains <text> (case-insensitive);
                           catches content whose text is not innerText (img alt) or renders late
          --click <text>   click the element whose accessible name matches <text> exactly
                           (case-sensitive) or contains it (case-insensitive fallback), then
                           wait for the accessibility tree to change; repeatable — clicks run
                           in order after load/waitfor
          --static-text    also print visible static text nodes (for GetByText authoring)
          --timeout <ms>   navigation/text/click timeout (default: {DefaultTimeoutMs})
          --headed         show the browser window instead of headless
          -h, --help       show this help

        Examples:
          dotnet run --project Scripts/Tools -- locator-dump https://github.com/Gredja/AiTest/issues/new --waitfor "Create new issue"
          dotnet run --project Scripts/Tools -- locator-dump https://github.com/Gredja/AiTest --anonymous
          dotnet run --project Scripts/Tools -- locator-dump https://github.com/o/r/issues/1 --click "Show options" --static-text
        """;

    private static string? ResolveStatePath(Options options)
    {
        if (options.IsAnonymous)
        {
            return null;
        }

        if (options.State is not null)
        {
            if (!File.Exists(options.State))
            {
                throw new LocatorDumpException($"state file not found: {options.State}");
            }

            return options.State;
        }

        return File.Exists(DefaultStatePath) ? DefaultStatePath : null;
    }

    private static string FindChromiumExecutable()
    {
        var overridePath = Environment.GetEnvironmentVariable("CHROMIUM_PATH");
        if (overridePath is not null)
        {
            if (File.Exists(overridePath))
            {
                return overridePath;
            }

            throw new LocatorDumpException($"CHROMIUM_PATH does not exist: {overridePath}");
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var msPlaywrightRoot = string.IsNullOrEmpty(localAppData)
            ? null
            : Path.Combine(localAppData, "ms-playwright");
        if (msPlaywrightRoot is null || !Directory.Exists(msPlaywrightRoot))
        {
            throw new LocatorDumpException(
                "ms-playwright browsers not found — run Scripts/install-playwright.ps1 or set CHROMIUM_PATH");
        }

        var executableName = OperatingSystem.IsWindows() ? "chrome.exe" : "chrome";
        var chromiumDirectories = Directory.GetDirectories(msPlaywrightRoot)
            .Select(Path.GetFileName)
            .Select(directory => (Directory: directory!, Match: ParseChromiumDirectory(directory!)))
            .Where(entry => entry.Match is not null)
            .OrderByDescending(entry => entry.Match)
            .ToList();

        foreach (var entry in chromiumDirectories)
        {
            foreach (var platformDirectory in _platformDirectories)
            {
                var candidate = Path.Combine(msPlaywrightRoot, entry.Directory, platformDirectory, executableName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        throw new LocatorDumpException(
            $"no chromium executable under {msPlaywrightRoot} — run Scripts/install-playwright.ps1");
    }

    private static long? ParseChromiumDirectory(string directory)
    {
        if (!directory.StartsWith("chromium-", StringComparison.Ordinal))
        {
            return null;
        }

        return long.TryParse(directory["chromium-".Length..], NumberStyles.None, CultureInfo.InvariantCulture,
            out var revision)
            ? revision
            : null;
    }

    private static List<Microsoft.Playwright.Cookie> LoadStorageStateCookies(string statePath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(statePath));
        var cookies = new List<Microsoft.Playwright.Cookie>();
        if (!document.RootElement.TryGetProperty("cookies", out var cookieElements)
            || cookieElements.ValueKind != JsonValueKind.Array)
        {
            return cookies;
        }

        foreach (var element in cookieElements.EnumerateArray())
        {
            var sameSiteText = element.TryGetProperty("sameSite", out var sameSiteElement)
                               && sameSiteElement.ValueKind == JsonValueKind.String
                ? sameSiteElement.GetString() ?? "Lax"
                : "Lax";
            Enum.TryParse<SameSiteAttribute>(sameSiteText, ignoreCase: true, out var sameSite);

            cookies.Add(new Microsoft.Playwright.Cookie
            {
                Name = element.GetProperty("name").GetString() ?? string.Empty,
                Value = element.GetProperty("value").GetString() ?? string.Empty,
                Domain = element.GetProperty("domain").GetString() ?? string.Empty,
                Path = element.TryGetProperty("path", out var pathElement) ? pathElement.GetString() ?? "/" : "/",
                Expires = element.TryGetProperty("expires", out var expiresElement)
                    ? (float)expiresElement.GetDouble()
                    : -1,
                HttpOnly = element.TryGetProperty("httpOnly", out var httpOnlyElement) && httpOnlyElement.GetBoolean(),
                Secure = element.TryGetProperty("secure", out var secureElement) && secureElement.GetBoolean(),
                SameSite = sameSite
            });
        }

        return cookies;
    }

    private static async Task<int> DumpAsync(Options options)
    {
        var statePath = ResolveStatePath(options);
        var executablePath = FindChromiumExecutable();
        Console.Error.WriteLine($"chromium: {executablePath}");
        Console.Error.WriteLine(statePath is not null ? $"state: {statePath}" : "state: anonymous");

        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !options.IsHeaded,
            ExecutablePath = executablePath,
            Args =
            [
                "--no-first-run",
                "--no-default-browser-check",
                "--disable-extensions",
                "--disable-sync",
                "--disable-translate",
                "--disable-background-networking",
                "--metrics-recording-only",
                "--mute-audio"
            ]
        });
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = ViewportWidth, Height = ViewportHeight }
        });

        if (statePath is not null)
        {
            var cookies = LoadStorageStateCookies(statePath);
            if (cookies.Count > 0)
            {
                await context.AddCookiesAsync([.. cookies]);
            }
        }

        var page = await context.NewPageAsync();

        Console.Error.WriteLine($"navigating: {options.Url}");
        try
        {
            await page.GotoAsync(options.Url!, new PageGotoOptions { Timeout = options.TimeoutMs });
        }
        catch (PlaywrightException exception)
        {
            throw new LocatorDumpException($"navigation failed: {exception.Message}");
        }

        if (options.WaitFor is not null)
        {
            await WaitForInnerTextAsync(page, options.WaitFor, options.TimeoutMs);
        }

        var session = await context.NewCDPSessionAsync(page);
        if (options.WaitForAx is not null)
        {
            await WaitForAxTextAsync(session, options.WaitForAx, options.TimeoutMs);
        }

        foreach (var clickText in options.Clicks)
        {
            await PerformClickAsync(session, clickText, options.TimeoutMs);
        }

        var nodes = await GetAxNodesAsync(session);
        var lines = nodes
            .Select(node => FormatElement(node, options.IncludeStaticText))
            .Where(line => line is not null)
            .ToList();
        Console.WriteLine(string.Join('\n', lines));
        Console.Error.WriteLine($"dumped {lines.Count} elements");

        return 0;
    }

    private static async Task WaitForInnerTextAsync(IPage page, string text, int timeoutMs)
    {
        var expression =
            $"document.body ? document.body.innerText.toLowerCase().includes({JsonString(text.ToLowerInvariant())}) : false";
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        Exception? lastError = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                if (await page.EvaluateAsync<bool>(expression))
                {
                    return;
                }

                lastError = null;
            }
            catch (PlaywrightException exception)
            {
                lastError = exception;
            }

            await Task.Delay(PollIntervalMs);
        }

        var detail = lastError is not null ? $" (last error: {lastError.Message})" : string.Empty;
        throw new LocatorDumpException(
            $"timed out after {timeoutMs}ms waiting for text \"{text}\"{detail}");
    }

    private static async Task WaitForAxTextAsync(ICDPSession session, string text, int timeoutMs)
    {
        var needle = text.ToLowerInvariant();
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        Exception? lastError = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var nodes = await GetAxNodesAsync(session);
                var match = nodes.FirstOrDefault(node =>
                    AxName(node).ToLowerInvariant().Contains(needle, StringComparison.Ordinal));
                if (match.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null)
                {
                    Console.Error.WriteLine(
                        $"ax-text found in [{AxRole(match)}] {JsonString(AxName(match))}");
                    return;
                }

                lastError = null;
            }
            catch (PlaywrightException exception)
            {
                lastError = exception;
            }

            await Task.Delay(PollIntervalMs);
        }

        var detail = lastError is not null ? $" (last error: {lastError.Message})" : string.Empty;
        throw new LocatorDumpException(
            $"timed out after {timeoutMs}ms waiting for accessibility-tree text \"{text}\"{detail}");
    }

    private static async Task PerformClickAsync(ICDPSession session, string clickText, int timeoutMs)
    {
        // the click target may render after the tree first changes (late menus, pickers),
        // so keep looking for it until the deadline instead of failing on the first miss
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        JsonElement node = default;
        var found = false;
        while (DateTime.UtcNow < deadline)
        {
            node = await FindClickableNodeAsync(session, clickText);
            if (node.ValueKind is JsonValueKind.Object)
            {
                found = true;
                break;
            }

            await Task.Delay(PollIntervalMs);
        }

        if (!found)
        {
            throw new LocatorDumpException(
                $"--click \"{clickText}\": no visible clickable element whose accessible name contains it within {timeoutMs}ms");
        }

        Console.Error.WriteLine($"clicking: [{AxRole(node)}] {JsonString(AxName(node))}");
        var before = await TreeSignatureAsync(session);

        var clicked = false;
        if (node.TryGetProperty("backendDOMNodeId", out var backendNodeId)
            && backendNodeId.ValueKind is JsonValueKind.Number)
        {
            try
            {
                await ClickViaDomAsync(session, backendNodeId.GetInt32());
                clicked = true;
            }
            catch (PlaywrightException)
            {
                // fall back to a mouse click at the node bounds
            }
        }

        if (!clicked && !await ClickViaMouseAsync(session, node))
        {
            throw new LocatorDumpException(
                $"--click \"{clickText}\": cannot resolve a click position for the element");
        }

        Exception? lastError = null;
        DateTime? changedAt = null;
        string? lastChangedSignature = null;
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(PollIntervalMs);
            try
            {
                var after = await TreeSignatureAsync(session);
                if (after != before && changedAt is null)
                {
                    changedAt = DateTime.UtcNow;
                }

                // wait until the tree stops changing after the click: menus render their items
                // in a second frame, and a dump right after the first change would miss them
                if (changedAt is not null
                    && after == lastChangedSignature
                    && (DateTime.UtcNow - changedAt.Value).TotalMilliseconds >= PostClickSettleMs)
                {
                    return;
                }

                lastChangedSignature = after;
                lastError = null;
            }
            catch (PlaywrightException exception)
            {
                lastError = exception;
            }
        }

        var detail = lastError is not null ? $" (last error: {lastError.Message})" : string.Empty;
        if (changedAt is null)
        {
            throw new LocatorDumpException(
                $"--click \"{clickText}\": accessibility tree did not change within {timeoutMs}ms after the click{detail}");
        }

        throw new LocatorDumpException(
            $"--click \"{clickText}\": accessibility tree kept changing for {timeoutMs}ms after the click{detail}");
    }

    private static async Task<JsonElement> FindClickableNodeAsync(ICDPSession session, string text)
    {
        var nodes = await GetAxNodesAsync(session);
        var candidates = new List<JsonElement>();
        foreach (var node in nodes)
        {
            var role = AxRole(node);
            if (!_clickableRoles.Contains(role) || !IsVisibleAxNode(node))
            {
                continue;
            }

            var name = NormalizeWhitespace(AxName(node));
            if (name.Length > 0)
            {
                candidates.Add(node);
            }
        }

        // case-sensitive exact match first: page sidebars often carry an "Edit labels"
        // link next to the "Edit Labels" picker button, and a substring would hit the link
        foreach (var node in candidates)
        {
            if (NormalizeWhitespace(AxName(node)) == text)
            {
                return node;
            }
        }

        var needle = text.ToLowerInvariant();
        foreach (var node in candidates)
        {
            if (NormalizeWhitespace(AxName(node)).ToLowerInvariant().Contains(needle, StringComparison.Ordinal))
            {
                return node;
            }
        }

        return default;
    }

    private static async Task ClickViaDomAsync(ICDPSession session, int backendNodeId)
    {
        var resolved = await session.SendAsync("DOM.resolveNode",
            new Dictionary<string, object> { ["backendNodeId"] = backendNodeId });
        var objectId = resolved!.Value.GetProperty("object").GetProperty("objectId").GetString()!;
        try
        {
            await session.SendAsync("Runtime.callFunctionOn", new Dictionary<string, object>
            {
                ["objectId"] = objectId,
                ["functionDeclaration"] =
                    "function() { this.scrollIntoView({ block: \"center\" }); this.click(); return true }",
                ["returnByValue"] = true
            });
        }
        finally
        {
            try
            {
                await session.SendAsync("Runtime.releaseObject", new Dictionary<string, object>
                {
                    ["objectId"] = objectId
                });
            }
            catch (PlaywrightException)
            {
                // releasing a stale object must not mask the click result
            }
        }
    }

    private static async Task<bool> ClickViaMouseAsync(ICDPSession session, JsonElement node)
    {
        if (!node.TryGetProperty("bounds", out var bounds)
            || bounds.ValueKind is not JsonValueKind.Object
            || !TryGetBounds(bounds, out var x, out var y, out var width, out var height)
            || width <= 0
            || height <= 0)
        {
            return false;
        }

        var centerX = x + width / 2;
        var centerY = y + height / 2;
        await session.SendAsync("Input.dispatchMouseEvent", new Dictionary<string, object>
        {
            ["type"] = "mouseMoved",
            ["x"] = centerX,
            ["y"] = centerY
        });
        await session.SendAsync("Input.dispatchMouseEvent", new Dictionary<string, object>
        {
            ["type"] = "mousePressed",
            ["x"] = centerX,
            ["y"] = centerY,
            ["button"] = "left",
            ["clickCount"] = 1
        });
        await session.SendAsync("Input.dispatchMouseEvent", new Dictionary<string, object>
        {
            ["type"] = "mouseReleased",
            ["x"] = centerX,
            ["y"] = centerY,
            ["button"] = "left",
            ["clickCount"] = 1
        });

        return true;
    }

    private static bool TryGetBounds(JsonElement bounds, out double x, out double y, out double width,
        out double height)
    {
        x = 0;
        y = 0;
        width = 0;
        height = 0;
        return bounds.TryGetProperty("x", out var xElement) && xElement.ValueKind is JsonValueKind.Number
            && bounds.TryGetProperty("y", out var yElement) && yElement.ValueKind is JsonValueKind.Number
            && bounds.TryGetProperty("width", out var widthElement)
            && widthElement.ValueKind is JsonValueKind.Number
            && bounds.TryGetProperty("height", out var heightElement)
            && heightElement.ValueKind is JsonValueKind.Number
            && (x = xElement.GetDouble()) >= 0
            && (y = yElement.GetDouble()) >= 0
            && (width = widthElement.GetDouble()) >= 0
            && (height = heightElement.GetDouble()) >= 0;
    }

    private static async Task<List<JsonElement>> GetAxNodesAsync(ICDPSession session)
    {
        var tree = await session.SendAsync("Accessibility.getFullAXTree");
        var nodes = new List<JsonElement>();
        if (tree is not null && tree.Value.TryGetProperty("nodes", out var nodeArray)
            && nodeArray.ValueKind is JsonValueKind.Array)
        {
            nodes.AddRange(nodeArray.EnumerateArray());
        }

        return nodes;
    }

    private static async Task<string> TreeSignatureAsync(ICDPSession session)
    {
        var nodes = await GetAxNodesAsync(session);

        return string.Join('\n', nodes.Select(node => $"{AxRole(node)} :: {AxName(node)}"));
    }

    private static bool IsVisibleAxNode(JsonElement node) =>
        node.TryGetProperty("ignored", out var ignored) && ignored.ValueKind is JsonValueKind.True
            ? false
            : AxProperty(node, "hidden")?.ValueKind is not JsonValueKind.True;

    private static string? FormatElement(JsonElement node, bool includeStaticText)
    {
        var role = AxRole(node);
        if (role.Length == 0)
        {
            return null;
        }

        if (node.TryGetProperty("ignored", out var ignored) && ignored.ValueKind is JsonValueKind.True)
        {
            return null;
        }

        if (AxProperty(node, "hidden")?.ValueKind is JsonValueKind.True)
        {
            return null;
        }

        var isStaticText = role is "text" or "StaticText";
        if (!isStaticText && !_interestingRoles.Contains(role))
        {
            return null;
        }

        if (isStaticText && !includeStaticText)
        {
            return null;
        }

        var name = NormalizeWhitespace(AxName(node));
        if (name.Length == 0)
        {
            return null;
        }

        var details = new List<string>();
        var level = AxProperty(node, "level");
        if (level is { } levelValue && levelValue.ValueKind is not JsonValueKind.Null
                                      && levelValue.ValueKind is not JsonValueKind.Undefined)
        {
            details.Add($"level={FormatAxNumber(levelValue)}");
        }

        if (AxProperty(node, "disabled")?.ValueKind is JsonValueKind.True)
        {
            details.Add("disabled");
        }

        var checkedValue = AxProperty(node, "checked");
        if (checkedValue?.ValueKind is JsonValueKind.True)
        {
            details.Add("checked=true");
        }
        else if (checkedValue is { ValueKind: JsonValueKind.String } checkedElement
                 && checkedElement.GetString() == "mixed")
        {
            details.Add("checked=mixed");
        }

        if (AxProperty(node, "selected")?.ValueKind is JsonValueKind.True)
        {
            details.Add("selected");
        }

        if (AxProperty(node, "expanded")?.ValueKind is JsonValueKind.True)
        {
            details.Add("expanded");
        }

        var detailText = details.Count > 0 ? $" ({string.Join(", ", details)})" : string.Empty;

        return $"- [{role}] {JsonString(name)}{detailText}";
    }

    private static JsonElement? AxProperty(JsonElement node, string propertyName)
    {
        if (!node.TryGetProperty("properties", out var properties) || properties.ValueKind is not JsonValueKind.Array)
        {
            return null;
        }

        foreach (var entry in properties.EnumerateArray())
        {
            if (entry.TryGetProperty("name", out var entryName)
                && entryName.ValueKind is JsonValueKind.String
                && entryName.GetString() == propertyName
                && entry.TryGetProperty("value", out var axValue)
                && axValue.TryGetProperty("value", out var value))
            {
                return value;
            }
        }

        return null;
    }

    private static string AxRole(JsonElement node) =>
        node.TryGetProperty("role", out var role)
        && role.TryGetProperty("value", out var roleValue)
        && roleValue.ValueKind is JsonValueKind.String
            ? roleValue.GetString() ?? string.Empty
            : string.Empty;

    private static string AxName(JsonElement node) =>
        node.TryGetProperty("name", out var name)
        && name.TryGetProperty("value", out var nameValue)
        && nameValue.ValueKind is JsonValueKind.String
            ? nameValue.GetString() ?? string.Empty
            : string.Empty;

    private static string FormatAxNumber(JsonElement value)
    {
        if (value.ValueKind is not JsonValueKind.Number)
        {
            return value.ToString();
        }

        return value.TryGetInt64(out var integer)
            ? integer.ToString(CultureInfo.InvariantCulture)
            : value.GetDouble().ToString(CultureInfo.InvariantCulture);
    }

    private static string NormalizeWhitespace(string value) =>
        _whitespaceRegex.Replace(value, " ").Trim();

    private static string JsonString(string value) => JsonSerializer.Serialize(value, _jsonStringOptions);
}
