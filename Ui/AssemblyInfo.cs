// UI tests run sequentially regardless of NumberOfTestWorkers in .runsettings:
// all fixtures share one storageState file (.auth/github-ui-state.json) —
// a concurrent LoginSignIn would race with feature tests reading the state.
[assembly: Parallelizable(ParallelScope.None)]
