using System.Text.Json;
using DronePatterns.Models;
using DronePatterns.Parsing;
using DronePatterns.Utils;

namespace DronePatterns.Commands;

public class ReceiveCommand : ICommand
{
    public string Name => "RECEIVE";

    private readonly string stockFilePath =
        DataPaths.Stocks;

    private readonly string pieceCatalogFilePath =
        DataPaths.PieceCatalog;

    private readonly string droneCatalogFilePath =
        DataPaths.DroneCatalog;

    private readonly IQuantityListParser quantityListParser =
        new QuantityListParser();

    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR RECEIVE requires a stock list";
        }

        QuantityListParseResult parseResult =
            quantityListParser.Parse(arguments);

        if (!parseResult.IsSuccess)
        {
            return parseResult.Error;
        }

        StockData? stock = LoadStock();

        if (stock == null)
        {
            return "ERROR Unable to read stock data";
        }

        PieceCatalogData? pieceCatalog =
            LoadPieceCatalog();

        if (pieceCatalog == null)
        {
            return "ERROR Unable to read piece catalog";
        }

        DroneCatalogData? droneCatalog =
            LoadDroneCatalog();

        if (droneCatalog == null)
        {
            return "ERROR Unable to read drone catalog";
        }

        List<ResolvedStockItem> resolvedItems =
            new();

        foreach (var item in parseResult.Items)
        {
            (
                ResolvedStockItem? ResolvedItem,
                string Error
            ) resolution = ResolveStockItem(
                item.Key,
                item.Value,
                stock,
                pieceCatalog,
                droneCatalog
            );

            if (resolution.ResolvedItem == null)
            {
                return resolution.Error;
            }

            AddOrMergeResolvedItem(
                resolvedItems,
                resolution.ResolvedItem
            );
        }

        foreach (
            ResolvedStockItem resolvedItem
            in resolvedItems
        )
        {
            AddToStock(
                stock,
                resolvedItem
            );
        }

        SaveStock(stock);

        return "STOCK_UPDATED";
    }

    private static (
        ResolvedStockItem? ResolvedItem,
        string Error
    ) ResolveStockItem(
        string itemName,
        int quantity,
        StockData stock,
        PieceCatalogData pieceCatalog,
        DroneCatalogData droneCatalog
    )
    {
        if (pieceCatalog.Pieces.ContainsKey(itemName))
        {
            return (
                new ResolvedStockItem
                {
                    Name = itemName,
                    Quantity = quantity,
                    Type = StockItemType.Piece
                },
                ""
            );
        }

        if (droneCatalog.Drones.ContainsKey(itemName))
        {
            return (
                new ResolvedStockItem
                {
                    Name = itemName,
                    Quantity = quantity,
                    Type = StockItemType.Drone
                },
                ""
            );
        }

        if (stock.Assemblies.ContainsKey(itemName))
        {
            return (
                new ResolvedStockItem
                {
                    Name = itemName,
                    Quantity = quantity,
                    Type = StockItemType.Assembly
                },
                ""
            );
        }

        (
            string? NormalizedAssemblyName,
            string Error
        ) assemblyResult = NormalizeUnnamedAssembly(
            itemName,
            pieceCatalog,
            stock
        );

        if (assemblyResult.NormalizedAssemblyName != null)
        {
            return (
                new ResolvedStockItem
                {
                    Name =
                        assemblyResult.NormalizedAssemblyName,
                    Quantity = quantity,
                    Type = StockItemType.Assembly
                },
                ""
            );
        }

        if (!string.IsNullOrEmpty(assemblyResult.Error))
        {
            return (
                null,
                assemblyResult.Error
            );
        }

        return (
            null,
            $"ERROR `{itemName}` is not a recognized stock item"
        );
    }

    private static (
        string? NormalizedAssemblyName,
        string Error
    ) NormalizeUnnamedAssembly(
        string itemName,
        PieceCatalogData pieceCatalog,
        StockData stock
    )
    {
        bool startsWithBracket =
            itemName.StartsWith('[');

        bool endsWithBracket =
            itemName.EndsWith(']');

        if (!startsWithBracket && !endsWithBracket)
        {
            return (null, "");
        }

        if (!startsWithBracket || !endsWithBracket)
        {
            return (
                null,
                $"ERROR Invalid assembly notation `{itemName}`"
            );
        }

        string innerContent = itemName[1..^1];

        string[] components = innerContent.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries
        );

        if (components.Length < 2)
        {
            return (
                null,
                "ERROR An assembly must contain at least 2 items"
            );
        }

        foreach (string component in components)
        {
            bool isKnownPiece =
                pieceCatalog.Pieces.ContainsKey(component);

            bool isKnownAssembly =
                stock.Assemblies.ContainsKey(component);

            if (!isKnownPiece && !isKnownAssembly)
            {
                return (
                    null,
                    $"ERROR Unknown assembly component `{component}`"
                );
            }
        }

        string[] normalizedComponents = components
            .OrderBy(
                component => component,
                StringComparer.Ordinal
            )
            .ToArray();

        string normalizedName =
            $"[{string.Join(", ", normalizedComponents)}]";

        return (
            normalizedName,
            ""
        );
    }

    private static void AddOrMergeResolvedItem(
        List<ResolvedStockItem> resolvedItems,
        ResolvedStockItem newItem
    )
    {
        ResolvedStockItem? existingItem =
            resolvedItems.FirstOrDefault(
                item =>
                    item.Type == newItem.Type
                    && item.Name == newItem.Name
            );

        if (existingItem == null)
        {
            resolvedItems.Add(newItem);
            return;
        }

        int existingIndex =
            resolvedItems.IndexOf(existingItem);

        resolvedItems[existingIndex] =
            new ResolvedStockItem
            {
                Name = existingItem.Name,
                Type = existingItem.Type,
                Quantity =
                    existingItem.Quantity
                    + newItem.Quantity
            };
    }

    private static void AddToStock(
        StockData stock,
        ResolvedStockItem item
    )
    {
        Dictionary<string, int> targetStock =
            item.Type switch
            {
                StockItemType.Piece =>
                    stock.Pieces,

                StockItemType.Assembly =>
                    stock.Assemblies,

                StockItemType.Drone =>
                    stock.Drones,

                _ => throw new ArgumentOutOfRangeException()
            };

        if (targetStock.ContainsKey(item.Name))
        {
            targetStock[item.Name] += item.Quantity;
        }
        else
        {
            targetStock.Add(
                item.Name,
                item.Quantity
            );
        }
    }

    private StockData? LoadStock()
    {
        if (!File.Exists(stockFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(
            stockFilePath
        );

        return JsonSerializer.Deserialize<StockData>(
            json
        );
    }

    private PieceCatalogData? LoadPieceCatalog()
    {
        if (!File.Exists(pieceCatalogFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(
            pieceCatalogFilePath
        );

        return JsonSerializer.Deserialize<PieceCatalogData>(
            json
        );
    }

    private DroneCatalogData? LoadDroneCatalog()
    {
        if (!File.Exists(droneCatalogFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(
            droneCatalogFilePath
        );

        return JsonSerializer.Deserialize<DroneCatalogData>(
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
}