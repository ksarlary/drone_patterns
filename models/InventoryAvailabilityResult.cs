namespace DronePatterns.Models;

public class InventoryAvailabilityResult
{
    public bool IsSuccess { get; init; }

    public bool IsAvailable { get; init; }

    public string Error { get; init; } = "";

    public static InventoryAvailabilityResult Available()
    {
        return new InventoryAvailabilityResult
        {
            IsSuccess = true,
            IsAvailable = true
        };
    }

    public static InventoryAvailabilityResult Unavailable()
    {
        return new InventoryAvailabilityResult
        {
            IsSuccess = true,
            IsAvailable = false
        };
    }

    public static InventoryAvailabilityResult Failure(
        string error
    )
    {
        return new InventoryAvailabilityResult
        {
            IsSuccess = false,
            IsAvailable = false,
            Error = error
        };
    }
}