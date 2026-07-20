using System.Text.Json;
using DronePatterns.Utils;
using DronePatterns.Models;

namespace DronePatterns.Commands.internal_commands;

public class GetOutStockCommand
{
    private readonly string stockFilePath = DataPaths.Stocks;

    public string Execute(int quantity, string itemName, bool displayOnly = true)
    {
        if (quantity <= 0)
        {
            return "ERROR GET_OUT_STOCK quantity must be greater than 0";
        }

        if (string.IsNullOrWhiteSpace(itemName))
        {
            return "ERROR GET_OUT_STOCK requires an item name";
        }

        string instruction = $"GET_OUT_STOCK {quantity} {itemName}";

        if (displayOnly)
        {
            return instruction;
        }

        StockData? stock = LoadStock();

        if (stock == null)
        {
            return "ERROR Unable to read stock data";
        }

        bool updated = RemoveFromStock(stock, itemName, quantity);

        if (!updated)
        {
            return $"ERROR Not enough stock for `{itemName}`";
        }

        SaveStock(stock);

        return instruction;
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

    private void SaveStock(StockData stock)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(stock, options);

        File.WriteAllText(stockFilePath, json);
    }

    private bool RemoveFromStock(StockData stock, string itemName, int quantity)
    {
        if (!stock.Pieces.ContainsKey(itemName))
        {
            return false;
        }

        if (stock.Pieces[itemName] < quantity)
        {
            return false;
        }

        stock.Pieces[itemName] -= quantity;

        return true;
    }
   
}