using DronePatterns.Models;
using DronePatterns.Parsing;
using DronePatterns.Services;

namespace DronePatterns.Commands;

public class ProduceCommand : ICommand
{
    public string Name => "PRODUCE";

    private readonly InventoryService inventoryService =
        new();

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
        
        InventoryOperationResult productionResult =
            inventoryService.Produce(
                neededStock,
                order,
                $"PRODUCE {arguments}"
            );

        if (!productionResult.IsSuccess)
        {
            return productionResult.Error;
        }

        return "STOCK_UPDATED";
    }
}