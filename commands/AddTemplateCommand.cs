using System.Text.Json;
using DronePatterns.Models;
using DronePatterns.Services;

namespace DronePatterns.Commands;

public class AddTemplateCommand : ICommand
{
	public string Name => "ADD_TEMPLATE";

	private readonly string pieceCatalogFilePath = "data/piece_catalog.json";
	private readonly string droneCatalogFilePath = "data/drone_catalog.json";
	private readonly string stockFilePath = "data/stocks.json";

	private readonly DroneCategoryService droneCategoryService = new();

	public string Execute(string arguments)
	{
		if (string.IsNullOrWhiteSpace(arguments))
		{
			return "ERROR Invalid template format";
		}

		string[] parts = arguments.Split(',', StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 7)
		{
			return "ERROR Invalid template format";
		}

		string templateName = parts[0].Trim();

		if (string.IsNullOrWhiteSpace(templateName))
		{
			return "ERROR Invalid template format";
		}

		PieceCatalogData? pieceCatalog = LoadPieceCatalog();

		if (pieceCatalog == null)
		{
			return "ERROR Unable to read piece catalog";
		}

        List<string> pieceNames = new();

        for (int i = 1; i < parts.Length; i++)
        {
            pieceNames.Add(parts[i].Trim());
        }

        foreach (string pieceName in pieceNames)
		{
			if (!pieceCatalog.Pieces.ContainsKey(pieceName))
			{
				return $"ERROR `{pieceName}` is not a recognized piece";
			}
		}

		DroneTemplate? drone = BuildDroneTemplate(
			templateName,
			pieceNames,
			pieceCatalog.Pieces
		);

		if (drone == null)
		{
			return "ERROR Invalid template pieces";
		}

        bool isValidCategory = droneCategoryService.IsValidDroneCategory(
            drone,
			pieceCatalog.Pieces
		);

        if (!isValidCategory)
        {
			return "ERROR Invalid drone category";
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

        string json = JsonSerializer.Serialize(droneCatalog, options);

        File.WriteAllText(droneCatalogFilePath, json);
    }

    private void SaveStock(StockData stock)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(stock, options);

        File.WriteAllText(stockFilePath, json);
    }

    private DroneTemplate? BuildDroneTemplate(
		string templateName,
		List<string> pieceNames,
		Dictionary<string, PieceDefinition> pieces
	)
	{
		Dictionary<string, string> piecesByFamily = new();

		foreach (string pieceName in pieceNames)
		{
			string family = pieces[pieceName].Family;

			if (piecesByFamily.ContainsKey(family))
			{
				return null;
			}

			piecesByFamily.Add(family, pieceName);
		}

		string[] requiredFamilies =
		{
			"Hull",
			"Core",
			"System",
			"Generator",
			"Move",
			"Processor"
		};

		foreach (string family in requiredFamilies)
		{
			if (!piecesByFamily.ContainsKey(family))
			{
				return null;
			}
		}

		return new DroneTemplate
		{
			Name = templateName,
			Hull = piecesByFamily["Hull"],
			Core = piecesByFamily["Core"],
			System = piecesByFamily["System"],
			Generator = piecesByFamily["Generator"],
			Move = piecesByFamily["Move"],
			Processor = piecesByFamily["Processor"]
		};
	}

	private void AddDroneToCatalog(DroneCatalogData droneCatalog, DroneTemplate drone)
	{
		droneCatalog.Drones.Add(
			drone.Name,
			new DroneDefinition
			{
				Hull = drone.Hull,
				Core = drone.Core,
				System = drone.System,
				Generator = drone.Generator,
				Move = drone.Move,
				Processor = drone.Processor
			}
		);
	}

	private void AddDroneToStock(StockData stock, string templateName)
	{
		if (!stock.Drones.ContainsKey(templateName))
		{
			stock.Drones.Add(templateName, 0);
		}
	}

	private class PieceCatalogData
	{
		public Dictionary<string, PieceDefinition> Pieces { get; set; } = new();
	}

	private class DroneCatalogData
	{
		public Dictionary<string, DroneDefinition> Drones { get; set; } = new();
	}

	private class DroneDefinition
	{
		public string Hull { get; set; } = "";
		public string Core { get; set; } = "";
		public string System { get; set; } = "";
		public string Generator { get; set; } = "";
		public string Move { get; set; } = "";
		public string Processor { get; set; } = "";
	}

	private class StockData
	{
		public Dictionary<string, int> Drones { get; set; } = new();
		public Dictionary<string, int> Pieces { get; set; } = new();
	}
}