namespace DronePatterns.Models;

public class OrderData
{
    public int NextOrderNumber { get; set; } = 1;

    public List<SalesOrder> Orders { get; set; } = new();
}