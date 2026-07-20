namespace DronePatterns.Models;

public class ResolvedStockItem
{
    public string Name { get; init; } = "";

    public int Quantity { get; init; }

    public StockItemType Type { get; init; }
}