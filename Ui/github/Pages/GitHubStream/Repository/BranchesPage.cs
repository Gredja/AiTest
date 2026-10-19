using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Github.Pages.GitHubStream.Repository;

internal sealed class BranchesPage(IPage page)
{
    private static readonly TimeSpan _deleteOutcomeTimeout = TimeSpan.FromSeconds(15);

    // The branches app hydrates up to ~5s after a reload; a click before that is swallowed
    // silently — no request at all (measured 2026-10-19). The interval must outlast hydration
    // yet stay above the delete round-trip (~1s), so a delete already in flight is never re-clicked.
    private static readonly TimeSpan _deleteRetryInterval = TimeSpan.FromSeconds(3);

    private readonly IPage _page = page;

    internal async Task OpenAsync() =>
        await _page.GotoAsync($"{TestConfig.UiBaseUrl}/{TestConfig.UiTestRepo}/branches");

    internal async Task<bool> ContainsBranchAsync(string branchName) =>
        await _branchLinks.Filter(new() { HasTextString = branchName }).CountAsync() > 0;

    internal async Task<WaitResult<bool>> WaitForBranchAsync(string branchName) =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                await _page.ReloadAsync();

                return await ContainsBranchAsync(branchName);
            },
            found => found);

    // The click can be swallowed while the branches app is still wiring up (2026-10-19: the first
    // click right after a reload sent no request at all, the next one deleted). The row keeps its
    // branch link after a successful delete — only a "Deleted" badge appears in it — so that badge
    // is the click post-condition, and the list itself goes stale until a reload (UI-13).
    internal async Task DeleteBranchAsync(string branchName)
    {
        var appeared = await WaitForBranchAsync(branchName);
        if (!appeared.IsSuccess)
        {
            throw new TimeoutException(
                $"branch must appear before delete (attempts={appeared.Attempts}, elapsed={appeared.Elapsed})");
        }

        // a branch can be listed in two sections (Your + Active branches), each with its
        // own Delete button — strict mode would trip on both; either button deletes the branch
        var row = _branchRows.Filter(new() { HasTextString = branchName }).First;
        await ClickDeleteUntilBadgeAsync(row);

        var gone = await WaitForBranchGoneAsync(branchName);
        if (!gone.IsSuccess)
        {
            throw new TimeoutException(
                $"branch must leave the list after delete (attempts={gone.Attempts}, elapsed={gone.Elapsed})");
        }
    }

    private async Task ClickDeleteUntilBadgeAsync(ILocator row)
    {
        var deletedBadge = row.GetByText("Deleted");

        var deleted = await WaitHelper.WaitUntilAsync(
            async () =>
            {
                if (await deletedBadge.CountAsync() > 0)
                {
                    return true;
                }

                await row.GetByRole(AriaRole.Button, new() { Name = "Delete branch" }).ClickAsync();

                return false;
            },
            isDeleted => isDeleted,
            timeout: _deleteOutcomeTimeout,
            interval: _deleteRetryInterval);

        if (!deleted.IsSuccess)
        {
            throw new TimeoutException(
                $"branch row must show Deleted after delete (attempts={deleted.Attempts}, elapsed={deleted.Elapsed})");
        }
    }

    // the list goes stale after a delete — the row may linger showing "Deleted" until a reload
    // pulls the fresh list (UI-13); poll rather than read the count once
    private async Task<WaitResult<bool>> WaitForBranchGoneAsync(string branchName) =>
        await WaitHelper.WaitUntilAsync(
            async () =>
            {
                await _page.ReloadAsync();

                return await ContainsBranchAsync(branchName);
            },
            isPresent => !isPresent);

    internal async Task CreateBranchAsync(string branchName)
    {
        await _newBranchButton.ClickAsync();
        await _branchNameTextbox.FillAsync(branchName);
        await _createBranchButton.ClickAsync();
        await _newBranchDialog.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    private readonly ILocator _branchLinks = page.GetByRole(AriaRole.Link);
    private readonly ILocator _branchRows = page.GetByRole(AriaRole.Row);
    private readonly ILocator _newBranchButton = page.GetByRole(AriaRole.Button, new() { Name = "New branch" });
    private readonly ILocator _branchNameTextbox = page.GetByRole(AriaRole.Textbox, new() { Name = "New branch name" });
    private readonly ILocator _createBranchButton = page.GetByRole(AriaRole.Button, new() { Name = "Create new branch" });
    private readonly ILocator _newBranchDialog = page.GetByRole(AriaRole.Dialog);
}
