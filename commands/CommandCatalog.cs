namespace DronePatterns.Commands;

public class CommandCatalog
{
    private readonly HashSet<string> availableCommands = new()
    {
        "STOCKS",
        "NEEDED_STOCKS",
        "INSTRUCTIONS",
        "VERIFY",
        "PRODUCE"
    };

    public string Evaluate(string userInput)
    {
        string commandName = ExtractCommandName(userInput);
        string feedback = $"";

        if (availableCommands.Contains(commandName))
        {
            switch(commandName)
            {
                case "STOCKS":
                    feedback += $"1 Drone1\n2 Drone2";
                    return feedback;
                case "NEEDED_STOCKS":
                    feedback += $"1 Drone1 :\n1 Piece1\n2 Piece2\n2 Drone2 :\n2 Piece1\n4 Piece2";
                    return feedback;
                case "INSTRUCTIONS":
                    feedback += $"PRODUCING Drone1\nASSEMBLY Drone1\nFINISHED Drone1";
                    return feedback;
                case "VERIFY":
                    feedback += $"AVAILABLE";
                    return feedback;
                case "PRODUCE":
                    feedback += $"STOCK_UPDATED";
                    return feedback;
            }
        }

        return $"ERROR Unknown command: {commandName}";
    }

    private string ExtractCommandName(string userInput)
    {
        string trimmedInput = userInput.Trim();

        string[] parts = trimmedInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts[0].ToUpper();
    }
}