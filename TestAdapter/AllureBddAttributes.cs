namespace TestAdapter;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllureEpicAttribute(string epic) : Attribute
{
    public string Epic { get; } = epic;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllureFeatureAttribute(string feature) : Attribute
{
    public string Feature { get; } = feature;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllureStoryAttribute(string story) : Attribute
{
    public string Story { get; } = story;
}
