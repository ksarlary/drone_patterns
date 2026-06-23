using DronePatterns.Models;
using DronePatterns.Strategies;

namespace DronePatterns.Services;

public class DroneCategoryService
{
	private readonly List<ICategoryStrategy> categoryStrategies = new()
	{
		new AerialCategoryStrategy(),
		new MarineCategoryStrategy(),
		new LandCategoryStrategy(),
		new SubmersibleCategoryStrategy()
	};

	public List<string> GetCategories(
		DroneTemplate drone,
		Dictionary<string, PieceDefinition> pieces
	)
	{
		List<string> categories = new();

		foreach (ICategoryStrategy strategy in categoryStrategies)
		{
			if (strategy.IsMatch(drone, pieces))
			{
				categories.Add(strategy.CategoryName);
			}
		}

		return categories;
	}

	public bool IsValidDroneCategory(
		DroneTemplate drone,
		Dictionary<string, PieceDefinition> pieces
	)
	{
		return GetCategories(drone, pieces).Count > 0;
	}
}