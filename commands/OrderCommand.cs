using DronePatterns.Models;
using DronePatterns.Parsing;
using DronePatterns.Services;

namespace DronePatterns.Commands;

public class OrderCommand : ICommand
{
    public string Name => "ORDER";

    private readonly IQuantityListParser quantityListParser =
        new QuantityListParser();

    private readonly NeededStocksCommand neededStocksCommand =
        new();

    private readonly SalesOrderService salesOrderService =
        new();

    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR ORDER requires a drone list";
        }

        QuantityListParseResult parseResult =
            quantityListParser.Parse(arguments);

        if (!parseResult.IsSuccess)
        {
            return parseResult.Error;
        }

        string validationError =
            neededStocksCommand.ValidateArguments(arguments);

        if (!string.IsNullOrWhiteSpace(validationError))
        {
            return validationError;
        }

        OrderCreationResult creationResult =
            salesOrderService.CreateOrder(
                parseResult.Items
            );

        if (!creationResult.IsSuccess)
        {
            return creationResult.Error;
        }

        return creationResult.OrderId;
    }
}