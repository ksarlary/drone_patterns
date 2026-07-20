using DronePatterns.Models;
using DronePatterns.Parsing;
using DronePatterns.Services;
using DronePatterns.Utils;

namespace DronePatterns.Commands;

public class SendCommand : ICommand
{
    public string Name => "SEND";

    private readonly IQuantityListParser quantityListParser =
        new QuantityListParser();

    private readonly SalesOrderService salesOrderService =
        new();

    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return
                "ERROR SEND requires an order identifier and drones";
        }

        int firstCommaIndex = arguments.IndexOf(',');

        if (firstCommaIndex < 0)
        {
            return
                "ERROR Invalid SEND format. " +
                "Expected: SEND ORDERID, ARGS";
        }

        string orderId = arguments[..firstCommaIndex].Trim();

        string droneArguments =
            arguments[(firstCommaIndex + 1)..].Trim();

        if (string.IsNullOrWhiteSpace(orderId))
        {
            return "ERROR Missing order identifier";
        }

        QuantityListParseResult parseResult =
            quantityListParser.Parse(droneArguments);

        if (!parseResult.IsSuccess)
        {
            return parseResult.Error;
        }

        OrderShipmentResult shipmentResult =
            salesOrderService.Send(
                orderId,
                parseResult.Items,
                $"SEND {arguments}"
            );

        if (!shipmentResult.IsSuccess)
        {
            return shipmentResult.Error;
        }

        if (shipmentResult.IsCompleted)
        {
            return $"COMPLETED {shipmentResult.OrderId}";
        }

        return
            $"Remaining for {shipmentResult.OrderId} : " +
            QuantityListFormatter.Format(
                shipmentResult.Remaining
            );
    }
}