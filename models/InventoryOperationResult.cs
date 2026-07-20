namespace DronePatterns.Models;

public class InventoryOperationResult
{
    public bool IsSuccess { get; init; }

    public string Error { get; init; } = "";

    public static InventoryOperationResult Success()
    {
        return new InventoryOperationResult
        {
            IsSuccess = true
        };
    }

    public static InventoryOperationResult Failure(
        string error
    )
    {
        return new InventoryOperationResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}