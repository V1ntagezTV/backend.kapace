using Bogus;

namespace backend.Tests.Helpers;

internal static class TestHelpers 
{
    private static Faker faker = new();

    public static int GetRandomInt() => faker.Random.Int(0, int.MaxValue);
    public static string GetRandomString() => faker.Random.String2(length: 10);
}