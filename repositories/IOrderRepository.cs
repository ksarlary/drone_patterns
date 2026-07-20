using DronePatterns.Models;

namespace DronePatterns.Repositories;

public interface IOrderRepository
{
    OrderData? Load();

    bool Save(OrderData data);
}