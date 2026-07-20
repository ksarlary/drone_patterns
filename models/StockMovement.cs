namespace DronePatterns.Models;

public class StockMovement
{
    public string Id { get; init; } = "";

    public DateTimeOffset OccurredAtUtc { get; init; }

    public StockMovementOperation Operation { get; init; }

    public StockItemType ItemType { get; init; }

    public string ItemName { get; init; } = "";

    public int QuantityDelta { get; init; }

    public string SourceInstruction { get; init; } = "";
}