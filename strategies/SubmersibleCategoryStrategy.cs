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
        return HasTag(pieces, drone.Hull, "S")
            && HasTag(pieces, drone.Generator, "S")
            && HasTag(pieces, drone.Move, "S")
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