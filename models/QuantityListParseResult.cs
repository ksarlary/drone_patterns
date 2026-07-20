namespace DronePatterns.Models;

public class QuantityListParseResult
{
    public bool IsSuccess { get; init; }

    public Dictionary<string, int> Items { get; init; } = new();

    public string Error { get; init; } = "";

    public static QuantityListParseResult Success(
        Dictionary<string, int> items
    )
    {
        return new QuantityListParseResult
        {
            IsSuccess = true,
            Items = items
        };
    }

    public static QuantityListParseResult Failure(
        string error
    )
    {
        return new QuantityListParseResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}