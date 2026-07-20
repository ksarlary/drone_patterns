using DronePatterns.Models;
using DronePatterns.Repositories;

namespace DronePatterns.Services;

public class SalesOrderService
{
    private readonly IOrderRepository orderRepository;
    private readonly IStockRepository stockRepository;
    private readonly IMovementRepository movementRepository;

    public SalesOrderService()
        : this(
            new JsonOrderRepository(),
            new JsonStockRepository(),
            new JsonMovementRepository()
        )
    {
    }

    public SalesOrderService(
        IOrderRepository orderRepository,
        IStockRepository stockRepository,
        IMovementRepository movementRepository
    )
    {
        this.orderRepository = orderRepository;
        this.stockRepository = stockRepository;
        this.movementRepository = movementRepository;
    }

    public OrderCreationResult CreateOrder(
        IReadOnlyDictionary<string, int> requestedDrones
    )
    {
        if (requestedDrones.Count == 0)
        {
            return OrderCreationResult.Failure(
                "ERROR Order cannot be empty"
            );
        }

        foreach (var requestedDrone in requestedDrones)
        {
            if (requestedDrone.Value <= 0)
            {
                return OrderCreationResult.Failure(
                    $"ERROR Invalid quantity for `{requestedDrone.Key}`"
                );
            }
        }

        OrderData? data = orderRepository.Load();

        if (data == null)
        {
            return OrderCreationResult.Failure(
                "ERROR Unable to read sales orders"
            );
        }

        string orderId = GenerateOrderId(data);

        SalesOrder order = new()
        {
            Id = orderId,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Requested = new Dictionary<string, int>(
                requestedDrones,
                StringComparer.Ordinal
            ),
            Sent = new Dictionary<string, int>(
                StringComparer.Ordinal
            ),
            Status = SalesOrderStatus.Pending
        };

        data.Orders.Add(order);

        if (!orderRepository.Save(data))
        {
            return OrderCreationResult.Failure(
                "ERROR Unable to save sales order"
            );
        }

        return OrderCreationResult.Success(orderId);
    }

    public OrderShipmentResult Send(
        string orderId,
        IReadOnlyDictionary<string, int> dronesToSend,
        string sourceInstruction
    )
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return OrderShipmentResult.Failure(
                "ERROR Missing order identifier"
            );
        }

        if (dronesToSend.Count == 0)
        {
            return OrderShipmentResult.Failure(
                "ERROR No drones to send"
            );
        }

        OrderData? originalOrderData =
            orderRepository.Load();

        if (originalOrderData == null)
        {
            return OrderShipmentResult.Failure(
                "ERROR Unable to read sales orders"
            );
        }

        StockData? originalStock =
            stockRepository.Load();

        if (originalStock == null)
        {
            return OrderShipmentResult.Failure(
                "ERROR Unable to read stock data"
            );
        }

        OrderData updatedOrderData =
            CloneOrderData(originalOrderData);

        StockData updatedStock =
            CloneStock(originalStock);

        SalesOrder? order =
            updatedOrderData.Orders.FirstOrDefault(
                candidate => candidate.Id.Equals(
                    orderId,
                    StringComparison.OrdinalIgnoreCase
                )
            );

        if (order == null)
        {
            return OrderShipmentResult.Failure(
                $"ERROR Unknown order `{orderId}`"
            );
        }

        if (order.Status == SalesOrderStatus.Completed)
        {
            return OrderShipmentResult.Failure(
                $"ERROR Order `{order.Id}` is already completed"
            );
        }

        Dictionary<string, int> remainingBeforeSend =
            order.GetRemaining();

        foreach (var droneToSend in dronesToSend)
        {
            string droneName = droneToSend.Key;
            int quantity = droneToSend.Value;

            if (quantity <= 0)
            {
                return OrderShipmentResult.Failure(
                    $"ERROR Invalid quantity for `{droneName}`"
                );
            }

            if (!order.Requested.ContainsKey(droneName))
            {
                return OrderShipmentResult.Failure(
                    $"ERROR `{droneName}` is not part of order `{order.Id}`"
                );
            }

            int remainingQuantity =
                remainingBeforeSend.GetValueOrDefault(droneName);

            if (quantity > remainingQuantity)
            {
                return OrderShipmentResult.Failure(
                    $"ERROR Cannot send {quantity} `{droneName}`; " +
                    $"only {remainingQuantity} remain in order `{order.Id}`"
                );
            }

            int availableStock =
                updatedStock.Drones.GetValueOrDefault(droneName);

            if (availableStock < quantity)
            {
                return OrderShipmentResult.Failure(
                    $"ERROR Not enough constructed `{droneName}` in stock"
                );
            }
        }

        List<StockMovement> movements = new();

        foreach (var droneToSend in dronesToSend)
        {
            string droneName = droneToSend.Key;
            int quantity = droneToSend.Value;

            updatedStock.Drones[droneName] -= quantity;

            order.Sent[droneName] =
                order.Sent.GetValueOrDefault(droneName)
                + quantity;

            movements.Add(
                new StockMovement
                {
                    Id = Guid.NewGuid().ToString("N"),
                    OccurredAtUtc = DateTimeOffset.UtcNow,
                    Operation = StockMovementOperation.Send,
                    ItemType = StockItemType.Drone,
                    ItemName = droneName,
                    QuantityDelta = -quantity,
                    SourceInstruction = sourceInstruction
                }
            );
        }

        Dictionary<string, int> remainingAfterSend =
            order.GetRemaining();

        bool isCompleted =
            remainingAfterSend.Count == 0;

        order.Status = isCompleted
            ? SalesOrderStatus.Completed
            : SalesOrderStatus.PartiallySent;

        if (!stockRepository.Save(updatedStock))
        {
            return OrderShipmentResult.Failure(
                "ERROR Unable to save stock data"
            );
        }

        if (!orderRepository.Save(updatedOrderData))
        {
            stockRepository.Save(originalStock);

            return OrderShipmentResult.Failure(
                "ERROR Unable to save sales order"
            );
        }

        if (!movementRepository.Append(movements))
        {
            stockRepository.Save(originalStock);
            orderRepository.Save(originalOrderData);

            return OrderShipmentResult.Failure(
                "ERROR Unable to save stock movement history"
            );
        }

        return OrderShipmentResult.Success(
            order.Id,
            isCompleted,
            remainingAfterSend
        );
    }

    public List<SalesOrder>? GetOpenOrders()
    {
        OrderData? data = orderRepository.Load();

        if (data == null)
        {
            return null;
        }

        return data.Orders
            .Where(
                order =>
                    order.Status != SalesOrderStatus.Completed
                    && order.GetRemaining().Count > 0
            )
            .OrderBy(order => order.CreatedAtUtc)
            .ToList();
    }

    private static string GenerateOrderId(
        OrderData data
    )
    {
        while (true)
        {
            string candidate =
                $"ORD-{data.NextOrderNumber:D6}";

            data.NextOrderNumber++;

            bool alreadyExists = data.Orders.Any(
                order => order.Id.Equals(
                    candidate,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (!alreadyExists)
            {
                return candidate;
            }
        }
    }

    private static StockData CloneStock(
        StockData stock
    )
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

    private static OrderData CloneOrderData(
        OrderData data
    )
    {
        return new OrderData
        {
            NextOrderNumber = data.NextOrderNumber,

            Orders = data.Orders.Select(
                order => new SalesOrder
                {
                    Id = order.Id,
                    CreatedAtUtc = order.CreatedAtUtc,
                    Status = order.Status,

                    Requested =
                        new Dictionary<string, int>(
                            order.Requested,
                            StringComparer.Ordinal
                        ),

                    Sent =
                        new Dictionary<string, int>(
                            order.Sent,
                            StringComparer.Ordinal
                        )
                }
            ).ToList()
        };
    }
}