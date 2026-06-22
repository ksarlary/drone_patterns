namespace DronePatterns.Commands;

public class CommandCatalog
{
    private readonly Dictionary<string, ICommand> commands;

    public CommandCatalog()
    {
        List<ICommand> commandList = new()
        {
            new StocksCommand(),
            new NeededStocksCommand(),
            new InstructionsCommand(),
            new VerifyCommand(),
            new ProduceCommand()
        };

        commands = commandList.ToDictionary(
            command => command.Name,
            command => command
        );
    }



    public string Evaluate(string userInput)
    {
      
        string trimmedInput = userInput.Trim();
        string commandName = ExtractCommandName(trimmedInput);
        string arguments = ExtractArguments(trimmedInput);

        if (!commands.ContainsKey(commandName))
        {
            return $"ERROR Unknown command: {commandName}";
        }
        return commands[commandName].Execute(arguments);
    }

    private string ExtractCommandName(string userInput)
    {

        string[] parts = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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