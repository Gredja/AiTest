using Core.Attributes;
using Core.Models.Generic;

namespace Core.Models.JsonPlaceholder;

public class PhotoModelResponse : IdModel<int>
{
    public int AlbumId { get; set; }

    [RequiredField]
    public string Title { get; set; }

    [RequiredField]
    public string Url { get; set; }

    [RequiredField]
    public string ThumbnailUrl { get; set; }
}
