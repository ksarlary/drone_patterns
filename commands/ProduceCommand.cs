namespace DronePatterns.Commands;

public class ProduceCommand
{
    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR: command PRODUCE requires an order";
        }

        return $"STOCK_UPDATED";
    }
}