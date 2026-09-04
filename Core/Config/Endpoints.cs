namespace Core.Config;

public static class Endpoints
{
    public const string BaseUrl = "https://fakestoreapi.com";

    // Products
    public const string Products = "/products";
    /// <summary>Template — use AddUrlSegment("id", value)</summary>
    public const string ProductsById = "/products/{id}";

    // Carts
    public const string Carts = "/carts";
    /// <summary>Template — use AddUrlSegment("id", value)</summary>
    public const string CartsById = "/carts/{id}";

    // Users
    public const string Users = "/users";
    /// <summary>Template — use AddUrlSegment("id", value)</summary>
    public const string UsersById = "/users/{id}";

    // Auth
    public const string Login = "/auth/login";
}
