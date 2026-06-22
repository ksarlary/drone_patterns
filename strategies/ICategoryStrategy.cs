using DronePatterns.Models;

namespace DronePatterns.Strategies;

public interface ICategoryStrategy
{
    string CategoryName { get; }

    bool IsMatch(
        DroneTemplate drone,
        Dictionary<string, PieceDefinition> pieces
    );
}