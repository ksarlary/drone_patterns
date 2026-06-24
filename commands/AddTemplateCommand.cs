using System.Text.Json;
using DronePatterns.Models;
using DronePatterns.Services;

namespace DronePatterns.Commands;

public class AddTemplateCommand : ICommand
{
	public string Name => "ADD_TEMPLATE";

	private readonly string pieceCatalogFilePath = "data/piece_catalog.json";
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

        return $"TEMPLATE_VALID {templateName}";
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

	private class PieceCatalogData
	{
		public Dictionary<string, PieceDefinition> Pieces { get; set; } = new();
	}
}