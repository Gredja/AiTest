namespace Core.Config;

public static class Endpoints
{
    public static string BaseUrl => TestConfig.BaseUrl;
    public static int ExpectedProductCount => TestConfig.ExpectedProductCount;
    public static int ExpectedUserCount => TestConfig.ExpectedUserCount;
    public static int MaxResponseTimeMs => TestConfig.MaxResponseTimeMs;
    public const int TestProductId = 1;
    public const int TestUserId = 1;
    public const int TestCartId = 1;
    public const int ExpectedCartCount = 7;
    public const int ExpectedCategoryCount = 4;
    public const string TestCategoryName = "electronics";
    public const int ExpectedProductsInCategoryCount = 6;

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
