using DronePatterns.Models;

namespace DronePatterns.Repositories;

public interface IStockRepository
{
    StockData? Load();

    bool Save(StockData stock);
}