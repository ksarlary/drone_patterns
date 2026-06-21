namespace DronePatterns.Utils;

public static class ConsoleErrorHandler
{
    public static void WriteError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return;
        }

        if (errorMessage.StartsWith("ERROR"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR");
            Console.ResetColor();

            string remainingMessage = errorMessage.Substring("ERROR".Length);

            Console.WriteLine(remainingMessage);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR ");
            Console.ResetColor();

            Console.WriteLine(errorMessage);
        }
    }
}