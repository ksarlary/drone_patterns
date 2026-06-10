namespace DronePatterns.Commands;

public class VerifyCommand
{
    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR: command VERIFY requires an order";
        }

        return $"AVAILABLE";
    }
}