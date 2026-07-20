using System.Text.Json;
using DronePatterns.Utils;
using DronePatterns.Models;
using DronePatterns.Parsing;

namespace DronePatterns.Commands;

public class ProduceCommand : ICommand
{
    public string Name => "PRODUCE";

    private readonly string _stockFilePath = DataPaths.Stocks;

    private readonly VerifyCommand verifyCommand = new();
    private readonly NeededStocksCommand neededStocksCommand = new();
    
    private readonly IQuantityListParser quantityListParser =
        new QuantityListParser();

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

        QuantityListParseResult parseResult =
            quantityListParser.Parse(arguments);

        if (!parseResult.IsSuccess)
        {
            return parseResult.Error;
        }

        Dictionary<string, int> order =
            parseResult.Items;

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