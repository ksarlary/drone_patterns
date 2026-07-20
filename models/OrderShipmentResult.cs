namespace DronePatterns.Models;

public class OrderShipmentResult
{
    public bool IsSuccess { get; init; }

    public bool IsCompleted { get; init; }

    public string OrderId { get; init; } = "";

    public Dictionary<string, int> Remaining { get; init; } =
        new();

    public string Error { get; init; } = "";

    public static OrderShipmentResult Success(
        string orderId,
        bool isCompleted,
        Dictionary<string, int> remaining
    )
    {
        return new OrderShipmentResult
        {
            IsSuccess = true,
            OrderId = orderId,
            IsCompleted = isCompleted,
            Remaining = remaining
        };
    }

    public static OrderShipmentResult Failure(
        string error
    )
    {
        return new OrderShipmentResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}