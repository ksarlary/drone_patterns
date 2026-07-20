using System.Text.Json;
using DronePatterns.Utils;
using DronePatterns.Models;

namespace DronePatterns.Commands;

public class ProduceCommand : ICommand
{
    public string Name => "PRODUCE";

    private readonly string _stockFilePath = DataPaths.Stocks;

    private readonly VerifyCommand verifyCommand = new();
    private readonly NeededStocksCommand neededStocksCommand = new();

    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR PRODUCE requires an order";
        }

        string verifyResult = verifyCommand.Execute(arguments);

        if (verifyResult.StartsWith("ERROR"))
        {
            return verifyResult;
        }

        if (verifyResult == "UNAVAILABLE")
        {
            return "ERROR Not enough stock";
        }

        Dictionary<string, int>? order = ParseOrder(arguments);

        if (order == null)
        {
            return "ERROR Invalid order format";
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
            int quantity = neededItem.Value;

            stock.Pieces[itemName] -= quantity;
        }

        foreach (var producedDrone in order)
        {
            string droneName = producedDrone.Key;
            int quantity = producedDrone.Value;

            if (stock.Drones.ContainsKey(droneName))
            {
                stock.Drones[droneName] += quantity;
            }
            else
            {
                stock.Drones.Add(droneName, quantity);
            }
        }

        SaveStock(stock);

        return "STOCK_UPDATED";
    }

    private Dictionary<string, int>? ParseOrder(string arguments)
    {
        Dictionary<string, int> order = new();

        string[] orderParts = arguments.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (string orderPart in orderParts)
        {
            string cleanedPart = orderPart.Trim();

            string[] elements = cleanedPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (elements.Length != 2)
            {
                return null;
            }

            bool quantityIsValid = int.TryParse(elements[0], out int quantity);

            if (!quantityIsValid)
            {
                return null;
            }

            string droneName = elements[1];

            if (order.ContainsKey(droneName))
            {
                order[droneName] += quantity;
            }
            else
            {
                order.Add(droneName, quantity);
            }
        }

        return order;
    }

    private StockData? LoadStock()
    {
        if (!File.Exists(_stockFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(_stockFilePath);

        return JsonSerializer.Deserialize<StockData>(json);
    }

    private void SaveStock(StockData stock)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(stock, options);

        File.WriteAllText(_stockFilePath, json);
    }
}