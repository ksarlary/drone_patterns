using System.Text.Json;
using System.Text.Json.Serialization;
using DronePatterns.Models;
using DronePatterns.Utils;

namespace DronePatterns.Repositories;

public class JsonOrderRepository : IOrderRepository
{
    private readonly string filePath;

    private static readonly JsonSerializerOptions ReadOptions =
        CreateSerializerOptions(writeIndented: false);

    private static readonly JsonSerializerOptions WriteOptions =
        CreateSerializerOptions(writeIndented: true);

    public JsonOrderRepository()
        : this(DataPaths.Orders)
    {
    }

    public JsonOrderRepository(string filePath)
    {
        this.filePath = filePath;
    }

    public OrderData? Load()
    {
        if (!File.Exists(filePath))
        {
            return new OrderData();
        }

        try
        {
            string json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<OrderData>(
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

    public bool Save(OrderData data)
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