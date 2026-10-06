using Core.Models.FakeStore;

namespace Api.FakeStore.Helpers;

public static class FakeStoreTestData
{
    public const string OutageIgnoreReason = "FakeStoreAPI: service under investigation — outage HTTP 521 (fakestoreapi.com down), checked 2026-10-06; un-ignore all FakeStore tests after investigation";

    public static readonly ProductModelRequest TestProduct = new()
    {
        Title = "Test Product for contract",
        Price = 9.99m,
        Description = "Guarantee data",
        Category = "electronics",
        Image = "https://test.com/image.jpg"
    };

    public static readonly UserModelRequest TestUser = new()
    {
        Email = "test@example.com",
        Username = "testuser",
        Password = "password123",
        Name = new UserName { Firstname = "Test", Lastname = "User" },
        Address = new Address
        {
            City = "Test City",
            Street = "Test Street",
            Number = 1,
            Zipcode = "12345",
            Geolocation = new Geolocation { Lat = "0.0", Longitude = "0.0" }
        }
    };
}
