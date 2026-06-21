namespace DronePatterns.Models;

public class StockData
{
    public Dictionary<string, int> Drones { get; set; } = new();
    public Dictionary<string, int> Pieces { get; set; } = new();
}