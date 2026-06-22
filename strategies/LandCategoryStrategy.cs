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
        return HasTag(pieces, drone.Move, "L")
            && HasTag(pieces, drone.System, "2D");
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