using DronePatterns.Models;
using DronePatterns.Services;

namespace DronePatterns.Commands;

public class VerifyCommand : ICommand
{
    public string Name => "VERIFY";

    private readonly NeededStocksCommand neededStocksCommand =
        new();

    private readonly InventoryService inventoryService =
        new();

    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR VERIFY requires an order";
        }

        string validationError =
            neededStocksCommand.ValidateArguments(arguments);

        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }

        Dictionary<string, int>? neededStock =
            neededStocksCommand.GetNeededStocks(arguments);

        if (neededStock == null)
        {
            return "ERROR Unable to calculate needed stock";
        }

        InventoryAvailabilityResult availabilityResult =
            inventoryService.CheckPieceAvailability(
                neededStock
            );

        if (!availabilityResult.IsSuccess)
        {
            return availabilityResult.Error;
        }

        return availabilityResult.IsAvailable
            ? "AVAILABLE"
            : "UNAVAILABLE";
    }
}