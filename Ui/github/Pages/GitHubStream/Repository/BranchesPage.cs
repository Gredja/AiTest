using Core.Config;
using Core.Helpers;
using Microsoft.Playwright;

namespace Ui.Github.Pages.GitHubStream.Repository;

internal sealed class BranchesPage(IPage page)
{
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

    // delete fires immediately without a confirmation dialog — the branch link disappears
    // from the row once the round-trip lands (the row may linger showing "Deleted").
    // The list itself needs reload-polling (async load) — reuse WaitForBranchAsync first.
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
        await row.GetByRole(AriaRole.Button, new() { Name = "Delete branch" }).ClickAsync();
        await _branchLinks.Filter(new() { HasTextString = branchName }).First.WaitForAsync(
            new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

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
