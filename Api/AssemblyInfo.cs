using NUnit.Framework;

// Fixtures run in parallel with each other; tests inside a fixture stay sequential.
// Worker count defaults to 1 — change via .runsettings (<NUnit><NumberOfTestWorkers>)
// or CLI: dotnet test -- NUnit.NumberOfTestWorkers=4
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(1)]
