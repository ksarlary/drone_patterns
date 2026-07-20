using DronePatterns.Models;
using DronePatterns.Services;

namespace DronePatterns.Commands;

public class StocksCommand : ICommand
{
    public string Name => "STOCKS";

    private readonly InventoryService inventoryService =
        new();

    public string Execute(string arguments)
    {
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR STOCKS does not accept arguments";
        }

        StockData? stock = inventoryService.GetStock();

        if (stock == null)
        {
            return "ERROR Unable to read stock data";
        }

        List<string> lines = new();

        foreach (var drone in stock.Drones)
        {
            lines.Add(
                $"{drone.Value} {drone.Key}"
            );
        }

        foreach (var assembly in stock.Assemblies)
        {
            lines.Add(
                $"{assembly.Value} {assembly.Key}"
            );
        }

        foreach (var piece in stock.Pieces)
        {
            lines.Add(
                $"{piece.Value} {piece.Key}"
            );
        }

        return string.Join(
            Environment.NewLine,
            lines
        );
    }
}