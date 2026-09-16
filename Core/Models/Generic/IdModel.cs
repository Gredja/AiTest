using Core.Attributes;

namespace Core.Models.Generic;

public class IdModel<T>
{
    [PositiveId]
    public T Id { get; set; }
}
