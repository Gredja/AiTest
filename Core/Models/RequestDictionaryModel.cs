namespace Core.Models;

public enum ParamType
{
    Header,
    Parameter,
    UrlSegment
}

public class RequestDictionaryModel
{
    public ParamType Type { get; set; }
    public string Key { get; set; }
    public object Value { get; set; }
}
