namespace Core.Config;

public static class Endpoints
{
    public const string BaseUrl = "https://fakestoreapi.com";

    // Products
    public const string Products = "/products";
    public const string ProductsById = "/products/{id}";

    // Carts
    public const string Carts = "/carts";
    public const string CartsById = "/carts/{id}";

    // Users
    public const string Users = "/users";
    public const string UsersById = "/users/{id}";

    // Auth
    public const string Login = "/auth/login";
}
