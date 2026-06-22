using System.Text.Json;

namespace DronePatterns.Commands;

public class VerifyCommand : ICommand
{
    public string Name => "VERIFY";

    private readonly string stockFilePath = "data/stocks.json";
    private readonly NeededStocksCommand neededStocksCommand = new();

    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR VERIFY requires an order";
        }

        string validationError = neededStocksCommand.ValidateArguments(arguments);

        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }

        Dictionary<string, int>? neededStock = neededStocksCommand.GetNeededStocks(arguments);

        if (neededStock == null)
        {
            return "ERROR Unable to calculate needed stock";
        }

        StockData? stock = LoadStock();

        if (stock == null)
        {
            return "ERROR Unable to read stock data";
        }

        foreach (var neededItem in neededStock)
        {
            string itemName = neededItem.Key;
            int neededQuantity = neededItem.Value;

            int availableQuantity = GetAvailableQuantity(itemName, stock);

            if (availableQuantity < neededQuantity)
            {
                return "UNAVAILABLE";
            }
        }

        return "AVAILABLE";
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

    private int GetAvailableQuantity(string itemName, StockData stock)
    {
        if (stock.Pieces.ContainsKey(itemName))
        {
            return stock.Pieces[itemName];
        }

        if (stock.Drones.ContainsKey(itemName))
        {
            return stock.Drones[itemName];
        }

        return 0;
    }

    private class StockData
    {
        public Dictionary<string, int> Drones { get; set; } = new();
        public Dictionary<string, int> Pieces { get; set; } = new();
    }
}