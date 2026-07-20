using System.Text.Json;
using DronePatterns.Models;
using DronePatterns.Services;
using DronePatterns.Utils;

namespace DronePatterns.Commands;

public class AddTemplateCommand : ICommand
{
    public string Name => "ADD_TEMPLATE";

    private readonly string pieceCatalogFilePath = DataPaths.PieceCatalog;
    private readonly string droneCatalogFilePath = DataPaths.DroneCatalog;
    private readonly string stockFilePath = DataPaths.Stocks;

    private readonly DroneCategoryService droneCategoryService = new();

    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR ADD_TEMPLATE requires a template name and pieces";
        }

        string[] parts = arguments.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries
        );
        
        if (parts.Length < 7 || parts.Length > 10)
        {
            return "ERROR Invalid template format";
        }

        string templateName = parts[0];

        if (string.IsNullOrWhiteSpace(templateName))
        {
            return "ERROR Template name cannot be empty";
        }

        PieceCatalogData? pieceCatalog = LoadPieceCatalog();

        if (pieceCatalog == null)
        {
            return "ERROR Unable to read piece catalog";
        }

        List<string> pieceNames = parts
            .Skip(1)
            .ToList();

        foreach (string pieceName in pieceNames)
        {
            if (!pieceCatalog.Pieces.ContainsKey(pieceName))
            {
                return $"ERROR `{pieceName}` is not a recognized piece";
            }
        }

        (DroneTemplate? drone, string constructionError) = BuildDroneTemplate(
            templateName,
            pieceNames,
            pieceCatalog.Pieces
        );

        if (drone == null)
        {
            return constructionError;
        }

        string compatibilityError = ValidateSystemCompatibility(
            drone,
            pieceCatalog.Pieces
        );

        if (!string.IsNullOrEmpty(compatibilityError))
        {
            return compatibilityError;
        }

        bool isValidCategory = droneCategoryService.IsValidDroneCategory(
            drone,
            pieceCatalog.Pieces
        );

        if (!isValidCategory)
        {
            return "ERROR The template does not match any drone category";
        }

        DroneCatalogData? droneCatalog = LoadDroneCatalog();

        if (droneCatalog == null)
        {
            return "ERROR Unable to read drone catalog";
        }

        if (droneCatalog.Drones.ContainsKey(templateName))
        {
            return $"ERROR `{templateName}` already exists";
        }

        StockData? stock = LoadStock();

        if (stock == null)
        {
            return "ERROR Unable to read stock data";
        }

        AddDroneToCatalog(droneCatalog, drone);
        AddDroneToStock(stock, templateName);

        SaveDroneCatalog(droneCatalog);
        SaveStock(stock);

        return $"TEMPLATE_ADDED {templateName}";
    }

    private (
        DroneTemplate? Drone,
        string Error
    ) BuildDroneTemplate(
        string templateName,
        List<string> pieceNames,
        Dictionary<string, PieceDefinition> pieces
    )
    {
        Dictionary<string, List<string>> piecesByFamily = pieceNames
            .GroupBy(pieceName => pieces[pieceName].Family)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

        string[] supportedFamilies =
        {
            "Hull",
            "Core",
            "System",
            "Generator",
            "Move",
            "Processor"
        };

        foreach (string family in piecesByFamily.Keys)
        {
            if (!supportedFamilies.Contains(family))
            {
                return (
                    null,
                    $"ERROR Unsupported piece family `{family}`"
                );
            }
        }

        string exactFamilyError = ValidateExactFamilyCount(
            piecesByFamily,
            "Hull"
        );

        if (!string.IsNullOrEmpty(exactFamilyError))
        {
            return (null, exactFamilyError);
        }

        exactFamilyError = ValidateExactFamilyCount(
            piecesByFamily,
            "Core"
        );

        if (!string.IsNullOrEmpty(exactFamilyError))
        {
            return (null, exactFamilyError);
        }

        exactFamilyError = ValidateExactFamilyCount(
            piecesByFamily,
            "System"
        );

        if (!string.IsNullOrEmpty(exactFamilyError))
        {
            return (null, exactFamilyError);
        }

        exactFamilyError = ValidateExactFamilyCount(
            piecesByFamily,
            "Processor"
        );

        if (!string.IsNullOrEmpty(exactFamilyError))
        {
            return (null, exactFamilyError);
        }

        List<string> generators = GetFamilyPieces(
            piecesByFamily,
            "Generator"
        );

        if (generators.Count < 1)
        {
            return (
                null,
                "ERROR A drone requires at least 1 generator"
            );
        }

        if (generators.Count > 2)
        {
            return (
                null,
                "ERROR A drone can contain at most 2 generators"
            );
        }

        List<string> movementModules = GetFamilyPieces(
            piecesByFamily,
            "Move"
        );

        if (movementModules.Count < 1)
        {
            return (
                null,
                "ERROR A drone requires at least 1 movement module"
            );
        }

        if (movementModules.Count > 3)
        {
            return (
                null,
                "ERROR A drone can contain at most 3 movement modules"
            );
        }

        if (movementModules.Count >= 2 && generators.Count != 2)
        {
            return (
                null,
                "ERROR A drone with at least 2 movement modules requires 2 generators"
            );
        }

        return (
            new DroneTemplate
            {
                Name = templateName,
                Hull = piecesByFamily["Hull"][0],
                Core = piecesByFamily["Core"][0],
                System = piecesByFamily["System"][0],
                Generators = generators,
                MovementModules = movementModules,
                Processor = piecesByFamily["Processor"][0]
            },
            ""
        );
    }

    private static string ValidateExactFamilyCount(
        Dictionary<string, List<string>> piecesByFamily,
        string family
    )
    {
        int count = GetFamilyPieces(
            piecesByFamily,
            family
        ).Count;

        if (count == 0)
        {
            return $"ERROR A drone requires exactly 1 {family} piece";
        }

        if (count > 1)
        {
            return $"ERROR A drone cannot contain more than 1 {family} piece";
        }

        return "";
    }

    private static List<string> GetFamilyPieces(
        Dictionary<string, List<string>> piecesByFamily,
        string family
    )
    {
        if (!piecesByFamily.TryGetValue(
                family,
                out List<string>? familyPieces
            ))
        {
            return new List<string>();
        }

        return familyPieces;
    }

    private static string ValidateSystemCompatibility(
        DroneTemplate drone,
        Dictionary<string, PieceDefinition> pieces
    )
    {
        HashSet<string> systemDimensions = GetDimensionTags(
            pieces[drone.System]
        );

        HashSet<string> coreDimensions = GetDimensionTags(
            pieces[drone.Core]
        );

        HashSet<string> processorDimensions = GetDimensionTags(
            pieces[drone.Processor]
        );

        if (systemDimensions.Count == 0)
        {
            return $"ERROR System `{drone.System}` has no supported dimension";
        }

        if (!systemDimensions.Overlaps(coreDimensions))
        {
            return
                $"ERROR Core `{drone.Core}` is not compatible with system `{drone.System}`";
        }

        if (!systemDimensions.Overlaps(processorDimensions))
        {
            return
                $"ERROR Processor `{drone.Processor}` is not compatible with system `{drone.System}`";
        }

        return "";
    }

    private static HashSet<string> GetDimensionTags(
        PieceDefinition piece
    )
    {
        return piece.Tags
            .Where(tag => tag is "2D" or "3D")
            .ToHashSet();
    }

    private PieceCatalogData? LoadPieceCatalog()
    {
        if (!File.Exists(pieceCatalogFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(pieceCatalogFilePath);

        return JsonSerializer.Deserialize<PieceCatalogData>(json);
    }

    private DroneCatalogData? LoadDroneCatalog()
    {
        if (!File.Exists(droneCatalogFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(droneCatalogFilePath);

        return JsonSerializer.Deserialize<DroneCatalogData>(json);
    }

    private StockData? LoadStock()
    {
        if (!File.Exists(stockFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(stockFilePath);

        return JsonSerializer.Deserialize<StockData>(json);
    }

    private void SaveDroneCatalog(DroneCatalogData droneCatalog)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(
            droneCatalog,
            options
        );

        File.WriteAllText(
            droneCatalogFilePath,
            json
        );
    }

    private void SaveStock(StockData stock)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(
            stock,
            options
        );

        File.WriteAllText(
            stockFilePath,
            json
        );
    }

    private static void AddDroneToCatalog(
        DroneCatalogData droneCatalog,
        DroneTemplate drone
    )
    {
        droneCatalog.Drones.Add(
            drone.Name,
            new DroneDefinition
            {
                Hull = drone.Hull,
                Core = drone.Core,
                System = drone.System,
                Generators = new List<string>(drone.Generators),
                MovementModules =
                    new List<string>(drone.MovementModules),
                Processor = drone.Processor
            }
        );
    }

    private static void AddDroneToStock(
        StockData stock,
        string templateName
    )
    {
        if (!stock.Drones.ContainsKey(templateName))
        {
            stock.Drones.Add(templateName, 0);
        }
    }
}