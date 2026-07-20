using DronePatterns.Models;
using DronePatterns.Services;
using DronePatterns.Utils;

namespace DronePatterns.Commands;

public class ListOrderCommand : ICommand
{
    public string Name => "LIST_ORDER";

    private readonly SalesOrderService salesOrderService =
        new();

    public string Execute(string arguments)
    {
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR LIST_ORDER does not accept arguments";
        }

        List<SalesOrder>? openOrders =
            salesOrderService.GetOpenOrders();

        if (openOrders == null)
        {
            return "ERROR Unable to read sales orders";
        }

        if (openOrders.Count == 0)
        {
            return "NO_PENDING_ORDERS";
        }

        return string.Join(
            Environment.NewLine,
            openOrders.Select(
                order =>
                    $"{order.Id}: " +
                    QuantityListFormatter.Format(
                        order.GetRemaining()
                    )
            )
        );
    }
}