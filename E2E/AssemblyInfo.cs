// Fixture parallelism also lives in .runsettings (NumberOfTestWorkers=1, per-run override
// used by test-parallel-rerun.ps1 with 8) — runsettings/CLI is the single knob; the attribute
// below is commented out so it can never defeat the CLI override
[assembly: Parallelizable(ParallelScope.Fixtures)]
// [assembly: LevelOfParallelism(1)]
