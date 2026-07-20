using System.Text.Json;
using DronePatterns.Models;
using DronePatterns.Utils;

namespace DronePatterns.Repositories;

public class JsonStockRepository : IStockRepository
{
    private readonly string filePath;

    private static readonly JsonSerializerOptions ReadOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private static readonly JsonSerializerOptions WriteOptions =
        new()
        {
            WriteIndented = true
        };

    public JsonStockRepository()
        : this(DataPaths.Stocks)
    {
    }

    public JsonStockRepository(string filePath)
    {
        this.filePath = filePath;
    }

    public StockData? Load()
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<StockData>(
                json,
                ReadOptions
            );
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public bool Save(StockData stock)
    {
        try
        {
            string? directoryPath =
                Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonSerializer.Serialize(
                stock,
                WriteOptions
            );

            File.WriteAllText(
                filePath,
                json
            );

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}