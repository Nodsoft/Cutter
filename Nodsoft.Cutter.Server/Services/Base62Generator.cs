namespace Nodsoft.Cutter.Server.Services;

/// <summary>
/// Provides generation of random strings using the Base62 character set.
/// </summary>
public static class Base62Generator
{
    private const string CharacterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private static readonly Random Random = new();

    /// <summary>
    /// Generates a random string of the specified length using the Base62 character set.
    /// </summary>
    /// <param name="length">The length of the string to generate.</param>
    /// <returns>A random string of the specified length.</returns>
    public static string GenerateString(int length)
    {
        Span<char> arr = stackalloc char[length];

        for (int i = 0; i < length; i++)
        {
            arr[i] = CharacterSet[Random.Next(CharacterSet.Length)];
        }

        return arr.ToString();
    }
}