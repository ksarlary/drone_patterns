namespace DronePatterns.Commands;

public class CommandCatalog
{
    private readonly StocksCommand stocksCommand = new();
    private readonly VerifyCommand verifyCommand = new();
    private readonly ProduceCommand produceCommand = new();
    private readonly NeededStocksCommand neededStocksCommand = new();
    private readonly InstructionsCommand instructionsCommand = new();
    
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
        string trimmedInput = userInput.Trim();
        string commandName = ExtractCommandName(userInput);
        string arguments = ExtractArguments(trimmedInput);
        string feedback = $"";

        if (availableCommands.Contains(commandName))
        {
            switch(commandName)
            {
                case "STOCKS":
                    return stocksCommand.Execute(arguments);
                case "NEEDED_STOCKS":
                    return neededStocksCommand.Execute(arguments);
                case "INSTRUCTIONS":
                    return instructionsCommand.Execute(arguments);
                case "VERIFY":
                    return verifyCommand.Execute(arguments);
                case "PRODUCE":
                    return produceCommand.Execute(arguments);
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
    
    private string ExtractArguments(string input)
    {
        string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            return "";
        }

        return parts[1].Trim();
    }
}