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
        return HasTag(pieces, drone.Move, "F")
            && HasTag(pieces, drone.System, "3D");
    }

    private bool HasTag(
        Dictionary<string, PieceDefinition> pieces,
        string pieceName,
        string tag
    )
    {
        return pieces.ContainsKey(pieceName)
            && pieces[pieceName].Tags.Contains(tag);
    }
}