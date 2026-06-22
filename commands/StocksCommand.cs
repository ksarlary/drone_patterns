using System.Text.Json;

namespace DronePatterns.Commands;

public class StocksCommand : ICommand
{
    public string Name => "STOCKS";

    private readonly string stockFilePath = "Data/stocks.json";

    public string Execute(string arguments)
    {
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR STOCKS does not accept arguments";
        }

        StockData? stock = LoadStock();

        if (stock == null)
        {
            return "ERROR Unable to read stock data";
        }

        List<string> lines = new();

        foreach (var drone in stock.Drones)
        {
            lines.Add($"{drone.Value} {drone.Key}");
        }

        foreach (var piece in stock.Pieces)
        {
            lines.Add($"{piece.Value} {piece.Key}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private StockData? LoadStock()
    {
        if (!File.Exists(stockFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(stockFilePath);

        return JsonSerializer.Deserialize<StockData>(json);
    }

    private class StockData
    {
        public Dictionary<string, int> Drones { get; set; } = new();
        public Dictionary<string, int> Pieces { get; set; } = new();
    }


}