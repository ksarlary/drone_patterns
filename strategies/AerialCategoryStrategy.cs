using DronePatterns.Models;

namespace DronePatterns.Strategies;

public class AerialCategoryStrategy : ICategoryStrategy
{
    public string CategoryName => "Aérien";

    public bool IsMatch(
        DroneTemplate drone,
        Dictionary<string, PieceDefinition> pieces
    )
    {
        bool hasFlyingMovementModule = drone.MovementModules.Any(
            movementModule => HasTag(pieces, movementModule, "F")
        );

        return hasFlyingMovementModule
               && HasTag(pieces, drone.System, "3D");
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