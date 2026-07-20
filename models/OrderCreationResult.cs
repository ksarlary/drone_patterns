namespace DronePatterns.Models;

public class OrderCreationResult
{
    public bool IsSuccess { get; init; }

    public string OrderId { get; init; } = "";

    public string Error { get; init; } = "";

    public static OrderCreationResult Success(
        string orderId
    )
    {
        return new OrderCreationResult
        {
            IsSuccess = true,
            OrderId = orderId
        };
    }

    public static OrderCreationResult Failure(
        string error
    )
    {
        return new OrderCreationResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}