using DronePatterns.Models;
using DronePatterns.Parsing;
using DronePatterns.Repositories;

namespace DronePatterns.Commands;

public class GetMovementsCommand : ICommand
{
    public string Name => "GET_MOVEMENTS";

    private readonly IMovementRepository movementRepository =
        new JsonMovementRepository();

    private readonly ItemNameListParser itemNameListParser =
        new();

    public string Execute(string arguments)
    {
        MovementData? data = movementRepository.Load();

        if (data == null)
        {
            return "ERROR Unable to read movement history";
        }

        IEnumerable<StockMovement> movements =
            data.Movements;

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            ItemNameListParseResult parseResult =
                itemNameListParser.Parse(arguments);

            if (!parseResult.IsSuccess)
            {
                return parseResult.Error;
            }

            HashSet<string> requestedItems = new(
                parseResult.Items,
                StringComparer.Ordinal
            );

            movements = movements.Where(
                movement =>
                    requestedItems.Contains(movement.ItemName)
            );
        }

        List<StockMovement> orderedMovements = movements
            .OrderBy(movement => movement.OccurredAtUtc)
            .ToList();

        if (orderedMovements.Count == 0)
        {
            return "NO_MOVEMENTS";
        }

        return string.Join(
            Environment.NewLine,
            orderedMovements.Select(FormatMovement)
        );
    }

    private static string FormatMovement(
        StockMovement movement
    )
    {
        string signedQuantity =
            movement.QuantityDelta >= 0
                ? $"+{movement.QuantityDelta}"
                : movement.QuantityDelta.ToString();

        return
            $"{movement.OccurredAtUtc:O} | " +
            $"{movement.Operation.ToString().ToUpperInvariant()} | " +
            $"{signedQuantity} | " +
            $"{movement.ItemType} | " +
            $"{movement.ItemName}";
    }
}