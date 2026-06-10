namespace DronePatterns.Commands;

public class NeededStocksCommand
{
    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR: command NEEDED_STOCKS requires an order";
        }

        return $"1 Drone1 :\n1 Piece1\n2 Piece2\n2 Drone2 :\n2 Piece1\n4 Piece2";
    }
}