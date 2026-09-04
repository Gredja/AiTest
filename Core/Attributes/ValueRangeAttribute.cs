namespace Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ValueRangeAttribute : Attribute
{
    public double Min { get; }
    public double Max { get; }

    public ValueRangeAttribute(double min = double.MinValue, double max = double.MaxValue)
    {
        Min = min;
        Max = max;
    }
}
