namespace Core.Config;

public static class Endpoints
{
    public const string BaseUrl = "https://fakestoreapi.com";
    public const int ExpectedProductCount = 20;
    public const int MaxResponseTimeMs = 5000;
    public const int TestProductId = 1;

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
