namespace Core.Models;

public class RequestDictionaryModel
{
    internal string Type { get; set; }
    internal string Key { get; set; }
    internal object Value { get; set; }

    public RequestDictionaryModel() { }

    public RequestDictionaryModel(string type, string key, object value)
    {
        Type = type;
        Key = key;
        Value = value;
    }

    public RequestDictionaryModel(string key, object value)
    {
        Key = key;
        Value = value;
    }
}
