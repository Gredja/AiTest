namespace Core.Helpers;

public static class DataGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string RandomString(int length)
    {
        var characters = new char[length];

        for (var index = 0; index < length; index++)
        {
            characters[index] = Alphabet[Random.Shared.Next(Alphabet.Length)];
        }

        return new string(characters);
    }

    // Upper bound is exclusive (Random.Shared.Next semantics) — hence the name
    public static int RandomIntExclusive(int min, int max) => Random.Shared.Next(min, max);
}
