namespace DronePatterns.Commands;

public class StocksCommand
{
    public string Execute(string arguments)
    {
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            return $"ERROR: command STOCKS does not accept arguments";
        }

        return "1 Drone1\n2 Drone2";
    }
}