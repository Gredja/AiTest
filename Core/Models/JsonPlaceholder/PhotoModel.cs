using Core.Attributes;

namespace Core.Models.JsonPlaceholder;

public class PhotoModel
{
    [PositiveId]
    public int Id { get; set; }

    public int AlbumId { get; set; }

    [RequiredField]
    public string Title { get; set; }

    [RequiredField]
    public string Url { get; set; }

    [RequiredField]
    public string ThumbnailUrl { get; set; }
}
