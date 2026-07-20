using System.Text.Json;
using System.Text.Json.Serialization;
using DronePatterns.Models;
using DronePatterns.Utils;

namespace DronePatterns.Repositories;

public class JsonMovementRepository : IMovementRepository
{
    private readonly string filePath;

    private static readonly JsonSerializerOptions ReadOptions =
        CreateSerializerOptions(writeIndented: false);

    private static readonly JsonSerializerOptions WriteOptions =
        CreateSerializerOptions(writeIndented: true);

    public JsonMovementRepository()
        : this(DataPaths.Movements)
    {
    }

    public JsonMovementRepository(string filePath)
    {
        this.filePath = filePath;
    }

    public MovementData? Load()
    {
        if (!File.Exists(filePath))
        {
            return new MovementData();
        }

        try
        {
            string json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<MovementData>(
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

    public bool Append(IEnumerable<StockMovement> movements)
    {
        List<StockMovement> movementList = movements.ToList();

        if (movementList.Count == 0)
        {
            return true;
        }

        MovementData? data = Load();

        if (data == null)
        {
            return false;
        }

        data.Movements.AddRange(movementList);

        return Save(data);
    }

    public bool Save(MovementData data)
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
                data,
                WriteOptions
            );

            File.WriteAllText(filePath, json);

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

    private static JsonSerializerOptions CreateSerializerOptions(
        bool writeIndented
    )
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = writeIndented
        };

        options.Converters.Add(
            new JsonStringEnumConverter()
        );

        return options;
    }
}