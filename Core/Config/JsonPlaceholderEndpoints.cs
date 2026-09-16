namespace Core.Config;

public static class JsonPlaceholderEndpoints
{
    public static string BaseUrl => TestConfig.JsonPlaceholderBaseUrl;

    public const string Posts = "/posts";
    public const string PostsById = "/posts/{id}";
    public const string PostsByUser = "/posts?userId={userId}";

    public const string Comments = "/comments";
    public const string CommentsById = "/comments/{id}";
    public const string CommentsByPost = "/comments?postId={postId}";

    public const string Albums = "/albums";
    public const string AlbumsById = "/albums/{id}";
    public const string AlbumsByUser = "/albums?userId={userId}";

    public const string Photos = "/photos";
    public const string PhotosById = "/photos/{id}";
    public const string PhotosByAlbum = "/photos?albumId={albumId}";

    public const string Todos = "/todos";
    public const string TodosById = "/todos/{id}";
    public const string TodosByUser = "/todos?userId={userId}";

    public const string Users = "/users";
    public const string UsersById = "/users/{id}";
    public const string UsersPosts = "/users/{id}/posts";
    public const string UsersTodos = "/users/{id}/todos";
    public const string UsersAlbums = "/users/{id}/albums";
}
