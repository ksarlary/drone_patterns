using DronePatterns.Models;
using DronePatterns.Repositories;

namespace DronePatterns.Services;

public class InventoryService
{
    private readonly IStockRepository stockRepository;
    private readonly IMovementRepository movementRepository;

    public InventoryService()
        : this(
            new JsonStockRepository(),
            new JsonMovementRepository()
        )
    {
    }

    public InventoryService(
        IStockRepository stockRepository
    )
        : this(
            stockRepository,
            new JsonMovementRepository()
        )
    {
    }

    public InventoryService(
        IStockRepository stockRepository,
        IMovementRepository movementRepository
    )
    {
        this.stockRepository = stockRepository;
        this.movementRepository = movementRepository;
    }

    public StockData? GetStock()
    {
        return stockRepository.Load();
    }

    public InventoryAvailabilityResult CheckPieceAvailability(
        IReadOnlyDictionary<string, int> requiredPieces
    )
    {
        StockData? stock = stockRepository.Load();

        if (stock == null)
        {
            return InventoryAvailabilityResult.Failure(
                "ERROR Unable to read stock data"
            );
        }

        foreach (var requiredPiece in requiredPieces)
        {
            string pieceName = requiredPiece.Key;
            int requiredQuantity = requiredPiece.Value;

            if (requiredQuantity <= 0)
            {
                return InventoryAvailabilityResult.Failure(
                    $"ERROR Invalid required quantity for `{pieceName}`"
                );
            }

            if (!stock.Pieces.TryGetValue(
                    pieceName,
                    out int availableQuantity
                ))
            {
                return InventoryAvailabilityResult.Unavailable();
            }

            if (availableQuantity < requiredQuantity)
            {
                return InventoryAvailabilityResult.Unavailable();
            }
        }

        return InventoryAvailabilityResult.Available();
    }

    public InventoryOperationResult Receive(
        IEnumerable<ResolvedStockItem> items,
        string sourceInstruction = "RECEIVE"
    )
    {
        List<ResolvedStockItem> receivedItems = items.ToList();

        if (receivedItems.Count == 0)
        {
            return InventoryOperationResult.Failure(
                "ERROR No stock items to receive"
            );
        }

        StockData? originalStock = stockRepository.Load();

        if (originalStock == null)
        {
            return InventoryOperationResult.Failure(
                "ERROR Unable to read stock data"
            );
        }

        StockData updatedStock = CloneStock(originalStock);
        List<StockMovement> movements = new();

        foreach (ResolvedStockItem item in receivedItems)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return InventoryOperationResult.Failure(
                    "ERROR Stock item name cannot be empty"
                );
            }

            if (item.Quantity <= 0)
            {
                return InventoryOperationResult.Failure(
                    $"ERROR Quantity for `{item.Name}` must be greater than 0"
                );
            }

            Dictionary<string, int> targetStock =
                GetTargetStock(updatedStock, item.Type);

            int currentQuantity =
                targetStock.GetValueOrDefault(item.Name);

            if (currentQuantity > int.MaxValue - item.Quantity)
            {
                return InventoryOperationResult.Failure(
                    $"ERROR Stock quantity is too large for `{item.Name}`"
                );
            }

            if (targetStock.ContainsKey(item.Name))
            {
                targetStock[item.Name] += item.Quantity;
            }
            else
            {
                targetStock.Add(item.Name, item.Quantity);
            }

            movements.Add(
                CreateMovement(
                    StockMovementOperation.Receive,
                    item.Type,
                    item.Name,
                    item.Quantity,
                    sourceInstruction
                )
            );
        }

        return CommitStockAndMovements(
            originalStock,
            updatedStock,
            movements
        );
    }

    public InventoryOperationResult Produce(
        IReadOnlyDictionary<string, int> consumedPieces,
        IReadOnlyDictionary<string, int> producedDrones,
        string sourceInstruction = "PRODUCE"
    )
    {
        if (consumedPieces.Count == 0)
        {
            return InventoryOperationResult.Failure(
                "ERROR Production requires pieces"
            );
        }

        if (producedDrones.Count == 0)
        {
            return InventoryOperationResult.Failure(
                "ERROR Production requires at least one drone"
            );
        }

        StockData? originalStock = stockRepository.Load();

        if (originalStock == null)
        {
            return InventoryOperationResult.Failure(
                "ERROR Unable to read stock data"
            );
        }

        StockData updatedStock = CloneStock(originalStock);
        List<StockMovement> movements = new();

        foreach (var consumedPiece in consumedPieces)
        {
            string pieceName = consumedPiece.Key;
            int quantity = consumedPiece.Value;

            if (quantity <= 0)
            {
                return InventoryOperationResult.Failure(
                    $"ERROR Invalid quantity for `{pieceName}`"
                );
            }

            if (!updatedStock.Pieces.TryGetValue(
                    pieceName,
                    out int availableQuantity
                ))
            {
                return InventoryOperationResult.Failure(
                    $"ERROR `{pieceName}` is not available in stock"
                );
            }

            if (availableQuantity < quantity)
            {
                return InventoryOperationResult.Failure(
                    $"ERROR Not enough stock for `{pieceName}`"
                );
            }
        }

        foreach (var producedDrone in producedDrones)
        {
            string droneName = producedDrone.Key;
            int quantity = producedDrone.Value;

            if (quantity <= 0)
            {
                return InventoryOperationResult.Failure(
                    $"ERROR Invalid quantity for `{droneName}`"
                );
            }

            int currentQuantity =
                updatedStock.Drones.GetValueOrDefault(droneName);

            if (currentQuantity > int.MaxValue - quantity)
            {
                return InventoryOperationResult.Failure(
                    $"ERROR Stock quantity is too large for `{droneName}`"
                );
            }
        }

        foreach (var consumedPiece in consumedPieces)
        {
            updatedStock.Pieces[consumedPiece.Key] -=
                consumedPiece.Value;

            movements.Add(
                CreateMovement(
                    StockMovementOperation.Produce,
                    StockItemType.Piece,
                    consumedPiece.Key,
                    -consumedPiece.Value,
                    sourceInstruction
                )
            );
        }

        foreach (var producedDrone in producedDrones)
        {
            if (updatedStock.Drones.ContainsKey(producedDrone.Key))
            {
                updatedStock.Drones[producedDrone.Key] +=
                    producedDrone.Value;
            }
            else
            {
                updatedStock.Drones.Add(
                    producedDrone.Key,
                    producedDrone.Value
                );
            }

            movements.Add(
                CreateMovement(
                    StockMovementOperation.Produce,
                    StockItemType.Drone,
                    producedDrone.Key,
                    producedDrone.Value,
                    sourceInstruction
                )
            );
        }

        return CommitStockAndMovements(
            originalStock,
            updatedStock,
            movements
        );
    }

    public InventoryOperationResult Remove(
        StockItemType itemType,
        string itemName,
        int quantity,
        StockMovementOperation operation =
            StockMovementOperation.GetOutStock,
        string sourceInstruction = "GET_OUT_STOCK"
    )
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return InventoryOperationResult.Failure(
                "ERROR Stock item name cannot be empty"
            );
        }

        if (quantity <= 0)
        {
            return InventoryOperationResult.Failure(
                "ERROR Quantity must be greater than 0"
            );
        }

        StockData? originalStock = stockRepository.Load();

        if (originalStock == null)
        {
            return InventoryOperationResult.Failure(
                "ERROR Unable to read stock data"
            );
        }

        StockData updatedStock = CloneStock(originalStock);

        Dictionary<string, int> targetStock =
            GetTargetStock(updatedStock, itemType);

        if (!targetStock.TryGetValue(
                itemName,
                out int currentQuantity
            ))
        {
            return InventoryOperationResult.Failure(
                $"ERROR `{itemName}` is not available in stock"
            );
        }

        if (currentQuantity < quantity)
        {
            return InventoryOperationResult.Failure(
                $"ERROR Not enough stock for `{itemName}`"
            );
        }

        targetStock[itemName] -= quantity;

        List<StockMovement> movements =
        [
            CreateMovement(
                operation,
                itemType,
                itemName,
                -quantity,
                sourceInstruction
            )
        ];

        return CommitStockAndMovements(
            originalStock,
            updatedStock,
            movements
        );
    }

    public InventoryOperationResult EnsureDroneEntry(
        string droneName
    )
    {
        if (string.IsNullOrWhiteSpace(droneName))
        {
            return InventoryOperationResult.Failure(
                "ERROR Drone name cannot be empty"
            );
        }

        StockData? stock = stockRepository.Load();

        if (stock == null)
        {
            return InventoryOperationResult.Failure(
                "ERROR Unable to read stock data"
            );
        }

        if (stock.Drones.ContainsKey(droneName))
        {
            return InventoryOperationResult.Success();
        }

        stock.Drones.Add(droneName, 0);

        if (!stockRepository.Save(stock))
        {
            return InventoryOperationResult.Failure(
                "ERROR Unable to save stock data"
            );
        }

        return InventoryOperationResult.Success();
    }

    private InventoryOperationResult CommitStockAndMovements(
        StockData originalStock,
        StockData updatedStock,
        IEnumerable<StockMovement> movements
    )
    {
        if (!stockRepository.Save(updatedStock))
        {
            return InventoryOperationResult.Failure(
                "ERROR Unable to save stock data"
            );
        }

        if (!movementRepository.Append(movements))
        {
            stockRepository.Save(originalStock);

            return InventoryOperationResult.Failure(
                "ERROR Unable to save stock movement history"
            );
        }

        return InventoryOperationResult.Success();
    }

    private static StockMovement CreateMovement(
        StockMovementOperation operation,
        StockItemType itemType,
        string itemName,
        int quantityDelta,
        string sourceInstruction
    )
    {
        return new StockMovement
        {
            Id = Guid.NewGuid().ToString("N"),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Operation = operation,
            ItemType = itemType,
            ItemName = itemName,
            QuantityDelta = quantityDelta,
            SourceInstruction = sourceInstruction
        };
    }

    private static StockData CloneStock(StockData stock)
    {
        return new StockData
        {
            Drones = new Dictionary<string, int>(
                stock.Drones,
                StringComparer.Ordinal
            ),
            Assemblies = new Dictionary<string, int>(
                stock.Assemblies,
                StringComparer.Ordinal
            ),
            Pieces = new Dictionary<string, int>(
                stock.Pieces,
                StringComparer.Ordinal
            )
        };
    }

    private static Dictionary<string, int> GetTargetStock(
        StockData stock,
        StockItemType itemType
    )
    {
        return itemType switch
        {
            StockItemType.Piece => stock.Pieces,
            StockItemType.Assembly => stock.Assemblies,
            StockItemType.Drone => stock.Drones,

            _ => throw new ArgumentOutOfRangeException(
                nameof(itemType),
                itemType,
                "Unsupported stock item type"
            )
        };
    }
}