using System.Text.Json;
using DronePatterns.Models;
using DronePatterns.Utils;
using DronePatterns.Parsing;

namespace DronePatterns.Commands;

public class NeededStocksCommand : ICommand 

{
    public string Name => "NEEDED_STOCKS";
    private readonly string catalogFilePath = DataPaths.DroneCatalog;

    private readonly IQuantityListParser quantityListParser =
        new QuantityListParser();
    
    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR NEEDED_STOCKS requires an order";
        }

        DroneCatalogData? catalog = LoadCatalog();

        if (catalog == null)
        {
            return "ERROR Unable to read drone catalog";
        }

        QuantityListParseResult parseResult =
            quantityListParser.Parse(arguments);

        if (!parseResult.IsSuccess)
        {
            return parseResult.Error;
        }

        Dictionary<string, int> order =
            parseResult.Items;

        string validationError = ValidateOrder(order, catalog);

        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }

        Dictionary<string, int> totalNeededStock = CalculateNeededStock(order, catalog);

        List<string> lines = new();

        foreach (var orderItem in order)
        {
            string droneName = orderItem.Key;
            int quantity = orderItem.Value;

            lines.Add($"{quantity} {droneName}:");

            Dictionary<string, int> droneNeededStock = CalculateNeededStockForDrone(
                droneName,
                quantity,
                catalog
            );

            foreach (var neededItem in droneNeededStock)
            {
                lines.Add($"{neededItem.Value} {neededItem.Key}");
            }
        }

        lines.Add("Total:");

        foreach (var neededItem in totalNeededStock)
        {
            lines.Add($"{neededItem.Value} {neededItem.Key}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public Dictionary<string, int>? GetNeededStocks(string arguments)
    {
        DroneCatalogData? catalog = LoadCatalog();

        if (catalog == null)
        {
            return null;
        }

        QuantityListParseResult parseResult =
            quantityListParser.Parse(arguments);

        if (!parseResult.IsSuccess)
        {
            return null;
        }

        Dictionary<string, int> order =
            parseResult.Items;

        string validationError = ValidateOrder(order, catalog);

        if (!string.IsNullOrEmpty(validationError))
        {
            return null;
        }

        return CalculateNeededStock(order, catalog);
    }

    public string ValidateArguments(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR Missing order";
        }

        DroneCatalogData? catalog = LoadCatalog();

        if (catalog == null)
        {
            return "ERROR Unable to read drone catalog";
        }

        QuantityListParseResult parseResult =
            quantityListParser.Parse(arguments);

        if (!parseResult.IsSuccess)
        {
            return parseResult.Error;
        }

        Dictionary<string, int> order =
            parseResult.Items;

        return ValidateOrder(order, catalog);
    }

    private DroneCatalogData? LoadCatalog()
    {
        if (!File.Exists(catalogFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(catalogFilePath);

        return JsonSerializer.Deserialize<DroneCatalogData>(json);
    }

    private string ValidateOrder(Dictionary<string, int> order, DroneCatalogData catalog)
    {
        foreach (var item in order)
        {
            string droneName = item.Key;
            int quantity = item.Value;

            if (quantity <= 0)
            {
                return $"ERROR Invalid quantity for `{droneName}`";
            }

            if (!catalog.Drones.ContainsKey(droneName))
            {
                return $"ERROR `{droneName}` is not a recognized drone";
            }
        }

        return "";
    }

    private Dictionary<string, int> CalculateNeededStock(
        Dictionary<string, int> order,
        DroneCatalogData catalog)
    {
        Dictionary<string, int> neededStock = new();

        foreach (var item in order)
        {
            string droneName = item.Key;
            int quantity = item.Value;

            Dictionary<string, int> droneNeededStock = CalculateNeededStockForDrone(
                droneName,
                quantity,
                catalog
            );

            foreach (var neededItem in droneNeededStock)
            {
                AddNeededItem(neededStock, neededItem.Key, neededItem.Value);
            }
        }

        return neededStock;
    }

    private Dictionary<string, int> CalculateNeededStockForDrone(
        string droneName,
        int quantity,
        DroneCatalogData catalog
    )
    {
        Dictionary<string, int> neededStock = new();
    
        DroneDefinition drone = catalog.Drones[droneName];
    
        AddNeededItem(
            neededStock,
            drone.Hull,
            quantity
        );
    
        AddNeededItem(
            neededStock,
            drone.Core,
            quantity
        );
    
        AddNeededItem(
            neededStock,
            drone.System,
            quantity
        );
    
        foreach (string generator in drone.Generators)
        {
            AddNeededItem(
                neededStock,
                generator,
                quantity
            );
        }
    
        foreach (string movementModule in drone.MovementModules)
        {
            AddNeededItem(
                neededStock,
                movementModule,
                quantity
            );
        }
    
        AddNeededItem(
            neededStock,
            drone.Processor,
            quantity
        );
    
        return neededStock;
    }

    private void AddNeededItem(Dictionary<string, int> neededStock, string itemName, int quantity)
    {
        if (neededStock.ContainsKey(itemName))
        {
            neededStock[itemName] += quantity;
        }
        else
        {
            neededStock.Add(itemName, quantity);
        }
    }
}