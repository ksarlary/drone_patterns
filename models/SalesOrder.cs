namespace DronePatterns.Models;

public class SalesOrder
{
    public string Id { get; set; } = "";

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Dictionary<string, int> Requested { get; set; } =
        new();

    public Dictionary<string, int> Sent { get; set; } =
        new();

    public SalesOrderStatus Status { get; set; }

    public Dictionary<string, int> GetRemaining()
    {
        Dictionary<string, int> remaining = new(
            StringComparer.Ordinal
        );

        foreach (var requestedItem in Requested)
        {
            int sentQuantity =
                Sent.GetValueOrDefault(requestedItem.Key);

            int remainingQuantity =
                requestedItem.Value - sentQuantity;

            if (remainingQuantity > 0)
            {
                remaining.Add(
                    requestedItem.Key,
                    remainingQuantity
                );
            }
        }

        return remaining;
    }
}