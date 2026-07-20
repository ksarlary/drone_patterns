using System.Text.Json;
using DronePatterns.Commands.internal_commands;
using DronePatterns.Models;
using DronePatterns.Utils;

namespace DronePatterns.Commands;

public class InstructionsCommand : ICommand
{
    public string Name => "INSTRUCTIONS";

    private readonly string catalogFilePath =
        DataPaths.DroneCatalog;

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

        string validationError = ValidateOrder(
            order,
            catalog
        );

        if (!string.IsNullOrEmpty(validationError))
        {
            return validationError;
        }

        List<string> lines = new();

        foreach (var orderItem in order)
        {
            string droneName = orderItem.Key;
            int quantity = orderItem.Value;

            for (int index = 0; index < quantity; index++)
            {
                DroneDefinition drone =
                    catalog.Drones[droneName];

                AddInstructionsForOneDrone(
                    lines,
                    droneName,
                    drone
                );
            }
        }

        return string.Join(
            Environment.NewLine,
            lines
        );
    }

    private void AddInstructionsForOneDrone(
        List<string> lines,
        string droneName,
        DroneDefinition drone
    )
    {
        lines.Add($"PRODUCING {droneName}");

        AddStockRemovalInstructions(
            lines,
            drone
        );

        lines.Add(
            installCommand.Execute(
                drone.System,
                drone.Core,
                displayOnly: true
            )
        );

        string installedCore =
            installCommand.GetInstalledPieceName(
                drone.System,
                drone.Core
            );

        string currentAssembly = drone.Hull;
        int temporaryAssemblyNumber = 1;
        
        foreach (string generator in drone.Generators)
        {
            currentAssembly = AddAssemblyStep(
                lines,
                currentAssembly,
                generator,
                temporaryAssemblyNumber
            );

            temporaryAssemblyNumber++;
        }
        
        currentAssembly = AddAssemblyStep(
            lines,
            currentAssembly,
            installedCore,
            temporaryAssemblyNumber
        );

        temporaryAssemblyNumber++;

        foreach (
            string movementModule in drone.MovementModules
        )
        {
            currentAssembly = AddAssemblyStep(
                lines,
                currentAssembly,
                movementModule,
                temporaryAssemblyNumber
            );

            temporaryAssemblyNumber++;
        }

        AddAssemblyStep(
            lines,
            currentAssembly,
            drone.Processor,
            temporaryAssemblyNumber
        );

        lines.Add($"FINISHED {droneName}");
    }

    private void AddStockRemovalInstructions(
        List<string> lines,
        DroneDefinition drone
    )
    {
        lines.Add(
            getOutStockCommand.Execute(
                1,
                drone.Hull,
                displayOnly: true
            )
        );

        lines.Add(
            getOutStockCommand.Execute(
                1,
                drone.Core,
                displayOnly: true
            )
        );

        lines.Add(
            getOutStockCommand.Execute(
                1,
                drone.System,
                displayOnly: true
            )
        );

        foreach (string generator in drone.Generators)
        {
            lines.Add(
                getOutStockCommand.Execute(
                    1,
                    generator,
                    displayOnly: true
                )
            );
        }

        foreach (
            string movementModule in drone.MovementModules
        )
        {
            lines.Add(
                getOutStockCommand.Execute(
                    1,
                    movementModule,
                    displayOnly: true
                )
            );
        }

        lines.Add(
            getOutStockCommand.Execute(
                1,
                drone.Processor,
                displayOnly: true
            )
        );
    }

    private string AddAssemblyStep(
        List<string> lines,
        string currentAssembly,
        string nextPiece,
        int temporaryAssemblyNumber
    )
    {
        string resultName =
            $"TMP{temporaryAssemblyNumber}";

        lines.Add(
            assembleCommand.Execute(
                resultName,
                currentAssembly,
                nextPiece
            )
        );

        return resultName;
    }

    private DroneCatalogData? LoadCatalog()
    {
        if (!File.Exists(catalogFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(
            catalogFilePath
        );

        return JsonSerializer.Deserialize<DroneCatalogData>(
            json
        );
    }

    private static Dictionary<string, int>? ParseOrder(
        string arguments
    )
    {
        Dictionary<string, int> order = new();

        string[] orderParts = arguments.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries
        );

        foreach (string orderPart in orderParts)
        {
            string cleanedPart = orderPart.Trim();

            string[] elements = cleanedPart.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );

            if (elements.Length != 2)
            {
                return null;
            }

            bool quantityIsValid = int.TryParse(
                elements[0],
                out int quantity
            );

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
                order.Add(
                    droneName,
                    quantity
                );
            }
        }

        return order;
    }

    private static string ValidateOrder(
        Dictionary<string, int> order,
        DroneCatalogData catalog
    )
    {
        foreach (var item in order)
        {
            string droneName = item.Key;
            int quantity = item.Value;

            if (quantity <= 0)
            {
                return
                    $"ERROR Invalid quantity for `{droneName}`";
            }

            if (!catalog.Drones.ContainsKey(droneName))
            {
                return
                    $"ERROR `{droneName}` is not a recognized drone";
            }
        }

        return "";
    }
}