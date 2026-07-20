using DronePatterns.Models;

namespace DronePatterns.Repositories;

public interface IMovementRepository
{
    MovementData? Load();

    bool Append(IEnumerable<StockMovement> movements);

    bool Save(MovementData data);
}