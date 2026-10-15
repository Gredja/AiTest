// UI tests run sequentially regardless of NumberOfTestWorkers in .runsettings:
// all fixtures share one storageState file (.auth/github-ui-state.json) —
// a concurrent LoginSignIn would race with feature tests reading the state.
// API/E2E follow NumberOfTestWorkers from .runsettings (currently 1).
[assembly: Parallelizable(ParallelScope.None)]
