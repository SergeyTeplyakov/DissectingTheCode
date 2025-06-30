// See https://aka.ms/new-console-template for more information

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// Console.WriteLine("Hello, World!");
ProcessRequest(new UserRequest());

foreach (var n in 1..5)
{
    // Prints 1, 2, 3, 4, 5
    Console.WriteLine(n);
}

static void ProcessRequest(UserRequest request)
{
    request.UserName.ThrowIfNull();

    // request.UserName is not null here!
    // The compiler knows that.
    string userName = request.UserName;
}

public class UserRequest
{
    public string? UserName { get; set; }
}


public static class Contract
{
    public static T ThrowIfNull<T>(
        [NotNull]
        this T? value,
        [CallerArgumentExpression("value")]
        string? parameterName = null)
    {
        if (value is null)
        {
            ThrowArgumentNullException(parameterName);
        }

        return value;
    }

    [DoesNotReturn]
    private static void ThrowArgumentNullException(string? paramName, string? message = null)
    {

        if (message is null)
        {
            throw new ArgumentNullException(paramName);
        }

        throw new ArgumentNullException(paramName, message);
    }
}

public static class IntExtensions
{
    public static IEnumerator<int> GetEnumerator(this int n)
    {
        for (int i = 0; i < n; i++)
            yield return i;
    }
}

public static class RangeExtensions
{
    public static IEnumerator<int> GetEnumerator(this Range range)
    {
        for (int i = range.Start.Value; i <= range.End.Value; i++)
        {
            yield return i;
        }
    }
}