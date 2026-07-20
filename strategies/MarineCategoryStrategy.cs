using DronePatterns.Models;

namespace DronePatterns.Strategies;

public class MarineCategoryStrategy : ICategoryStrategy
{
    public string CategoryName => "Marin";

    public bool IsMatch(
        DroneTemplate drone,
        Dictionary<string, PieceDefinition> pieces
    )
    {
        bool hasMarineMovementModule = drone.MovementModules.Any(
            movementModule => HasTag(pieces, movementModule, "M")
        );

        return HasTag(pieces, drone.Hull, "S")
            && hasMarineMovementModule
            && HasTag(pieces, drone.System, "2D");
    }

    private static bool HasTag(
        Dictionary<string, PieceDefinition> pieces,
        string pieceName,
        string tag
    )
    {
        return pieces.TryGetValue(pieceName, out PieceDefinition? piece)
            && piece.Tags.Contains(tag);
    }
}