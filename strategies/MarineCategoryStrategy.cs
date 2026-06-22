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
        return HasTag(pieces, drone.Hull, "S")
            && HasTag(pieces, drone.System, "2D")
            && HasTag(pieces, drone.Move, "M");
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