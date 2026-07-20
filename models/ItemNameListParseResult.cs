namespace DronePatterns.Models;

public class ItemNameListParseResult
{
    public bool IsSuccess { get; init; }

    public List<string> Items { get; init; } = new();

    public string Error { get; init; } = "";

    public static ItemNameListParseResult Success(
        List<string> items
    )
    {
        return new ItemNameListParseResult
        {
            IsSuccess = true,
            Items = items
        };
    }

    public static ItemNameListParseResult Failure(
        string error
    )
    {
        return new ItemNameListParseResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}