using DronePatterns.Models;

namespace DronePatterns.Strategies;

public class SubmersibleCategoryStrategy : ICategoryStrategy
{
    public string CategoryName => "Submersible";

    public bool IsMatch(
        DroneTemplate drone,
        Dictionary<string, PieceDefinition> pieces
    )
    {
        bool allGeneratorsAreSubmersible = drone.Generators.All(
            generator => HasTag(pieces, generator, "S")
        );

        bool allMovementModulesAreSubmersible = drone.MovementModules.All(
            movementModule => HasTag(pieces, movementModule, "S")
        );

        return HasTag(pieces, drone.Hull, "S")
               && allGeneratorsAreSubmersible
               && allMovementModulesAreSubmersible
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