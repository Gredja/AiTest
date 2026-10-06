namespace Core.Reporting;

internal sealed record TestRunRow(string Test, string Fixture, string Category, string Result, double Duration);
