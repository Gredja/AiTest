namespace AllureAdapter.Helpers;

internal class ContainerInfo
{
    public string Uuid { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Children { get; set; } = [];
}
