namespace Core.Config;

public static class JsonPlaceholderEndpoints
{
    public static string BaseUrl => TestConfig.JsonPlaceholderBaseUrl;
    public static int MaxResponseTimeMs => TestConfig.MaxResponseTimeMs;
    public static int ExpectedPostCount => TestConfig.ExpectedPostCount;
    public static int ExpectedTodoCount => TestConfig.ExpectedTodoCount;
    public static int ExpectedAlbumCount => TestConfig.ExpectedAlbumCount;
    public static int ExpectedCommentCount => TestConfig.ExpectedCommentCount;
    public static int ExpectedPhotoCount => TestConfig.ExpectedPhotoCount;
    public static int ExpectedUserCount => TestConfig.JsonPlaceholderExpectedUserCount;
    public const int TestPostId = 1;
    public const int TestUserId = 1;
    public const int TestAlbumId = 1;
    public const int TestTodoId = 1;
    public const int TestCommentId = 1;
    public const int TestPhotoId = 1;

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
