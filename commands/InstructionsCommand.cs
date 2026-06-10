namespace DronePatterns.Commands;

public class InstructionsCommand
{
    public string Execute(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return "ERROR: command INSTRUCTIONS requires an order";
        }

        return $"PRODUCING Drone1\nASSEMBLY Drone1\nFINISHED Drone1";
    }
}