using System.Text.Json;
using DronePatterns.Commands.internal_commands;

namespace DronePatterns.Commands;

public class InstructionsCommand : ICommand
{

    public string Name => "INSTRUCTIONS";
    private readonly string _catalogFilePath = "data/drone_catalog.json";

    private readonly GetOutStockCommand getOutStockCommand = new();
    private readonly InstallCommand installCommand = new();
    private readonly AssembleCommand assembleCommand = new();

    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR INSTRUCTIONS requires an order";
        }

        DroneCatalogData? catalog = LoadCatalog();

        if (catalog == null)
        {
            return "ERROR Unable to read drone catalog";
        }

        Dictionary<string, int>? order = ParseOrder(arguments);

        if (order == null)
        {
            return "ERROR Invalid order format";
        }

        string validationError = ValidateOrder(order, catalog);

        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }

        List<string> lines = new();

        foreach (var orderItem in order)
        {
            string droneName = orderItem.Key;
            int quantity = orderItem.Value;

            for (int i = 0; i < quantity; i++)
            {
                DroneDefinition drone = catalog.Drones[droneName];

                AddInstructionsForOneDrone(lines, droneName, drone);
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private void AddInstructionsForOneDrone(
        List<string> lines,
        string droneName,
        DroneDefinition drone)
    {
        lines.Add($"PRODUCING {droneName}");

        lines.Add(getOutStockCommand.Execute(1, drone.Hull, displayOnly: true));
        lines.Add(getOutStockCommand.Execute(1, drone.Core, displayOnly: true));
        lines.Add(getOutStockCommand.Execute(1, drone.Generator, displayOnly: true));
        lines.Add(getOutStockCommand.Execute(1, drone.Move, displayOnly: true));
        lines.Add(getOutStockCommand.Execute(1, drone.Processor, displayOnly: true));

        lines.Add(installCommand.Execute(drone.System, drone.Core, displayOnly: true));

        string installedCore = installCommand.GetInstalledPieceName(drone.System, drone.Core);

        lines.Add(assembleCommand.Execute("TMP1", drone.Hull, drone.Generator));
        lines.Add(assembleCommand.Execute("TMP2", "TMP1", drone.Move));

        lines.Add(assembleCommand.Execute("", "TMP2", installedCore));

        string unnamedAssembly = assembleCommand.GetUnnamedAssemblyName("TMP2", installedCore);

        lines.Add(assembleCommand.Execute("", unnamedAssembly, drone.Processor));

        lines.Add($"FINISHED {droneName}");
    }

    private DroneCatalogData? LoadCatalog()
    {
        if (!File.Exists(_catalogFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(_catalogFilePath);

        return JsonSerializer.Deserialize<DroneCatalogData>(json);
    }

    private Dictionary<string, int>? ParseOrder(string arguments)
    {
        Dictionary<string, int> order = new();

        string[] orderParts = arguments.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (string orderPart in orderParts)
        {
            string cleanedPart = orderPart.Trim();

            string[] elements = cleanedPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (elements.Length != 2)
            {
                return null;
            }

            bool quantityIsValid = int.TryParse(elements[0], out int quantity);

            if (!quantityIsValid)
            {
                return null;
            }

            string droneName = elements[1];

            if (order.ContainsKey(droneName))
            {
                order[droneName] += quantity;
            }
            else
            {
                order.Add(droneName, quantity);
            }
        }

        return order;
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
}