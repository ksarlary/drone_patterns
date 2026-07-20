using DronePatterns.Models;
using DronePatterns.Services;

namespace DronePatterns.Commands.internal_commands;

public class GetOutStockCommand
{
    private readonly InventoryService inventoryService =
        new();

    public string Execute(
        int quantity,
        string itemName,
        bool displayOnly = true
    )
    {
        if (quantity <= 0)
        {
            return
                "ERROR GET_OUT_STOCK quantity must be greater than 0";
        }

        if (string.IsNullOrWhiteSpace(itemName))
        {
            return
                "ERROR GET_OUT_STOCK requires an item name";
        }

        string instruction =
            $"GET_OUT_STOCK {quantity} {itemName}";

        if (displayOnly)
        {
            return instruction;
        }

        InventoryOperationResult removalResult =
            inventoryService.Remove(
                StockItemType.Piece,
                itemName,
                quantity,
                StockMovementOperation.GetOutStock,
                instruction
            );

        if (!removalResult.IsSuccess)
        {
            return removalResult.Error;
        }

        return instruction;
    }
}