namespace Core.Config;

public static class FakeStoreEndpoints
{
    public static string BaseUrl => TestConfig.FakeStoreBaseUrl;

    public const string Products = "/products";
    public const string ProductsById = "/products/{id}";
    public const string ProductsCategories = "/products/categories";
    public const string ProductsByCategory = "/products/category/{category}";

    public const string Carts = "/carts";
    public const string CartsById = "/carts/{id}";

    public const string Users = "/users";
    public const string UsersById = "/users/{id}";

    public const string Login = "/auth/login";
}
