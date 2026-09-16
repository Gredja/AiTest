namespace Core.Models.Generic;

public class IdNameModel<TId> : IdModel<TId>
{
    public string Name { get; set; }
}
