using DronePatterns.Models;

namespace DronePatterns.Strategies;

public class LandCategoryStrategy : ICategoryStrategy
{
    public string CategoryName => "Terrestre";

    public bool IsMatch(
        DroneTemplate drone,
        Dictionary<string, PieceDefinition> pieces
    )
    {
        bool hasLandMovementModule = drone.MovementModules.Any(
            movementModule => HasTag(pieces, movementModule, "L")
        );

        return hasLandMovementModule
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