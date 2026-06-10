namespace DronePatterns;
using DronePatterns.Commands;
using DronePatterns.Utils;

class Program
{
    static void Main(string[] args)
    {
        CommandCatalog commandCatalog = new();
        
        ConsoleHelper.WriteInfo("Welcome to the Drone Factory");
        ConsoleHelper.WriteInfo("Type a command or EXIT to quit");
        Console.WriteLine();
        
        while (true)
        {
            Console.Write("> ");
            string? userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                continue;
            }

            userInput = userInput.Trim();

            if (userInput.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelper.WriteInfo("Goodbye!");
                break;
            }

            string feedback = commandCatalog.Evaluate(userInput);
            
            if (feedback.StartsWith("ERROR"))
            {
                ConsoleHelper.WriteError(feedback);
            }
            else
            {
                ConsoleHelper.WriteSuccess(feedback);
            }
        }
    }
}