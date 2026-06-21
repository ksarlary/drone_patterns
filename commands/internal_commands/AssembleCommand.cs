namespace DronePatterns.Commands.internal_commands;

public class AssembleCommand
{
    public string Execute(string resultName, string firstPieceName, string secondPieceName)
    {
        if (string.IsNullOrWhiteSpace(firstPieceName))
        {
            return "ERROR ASSEMBLE requires a first piece";
        }

        if (string.IsNullOrWhiteSpace(secondPieceName))
        {
            return "ERROR ASSEMBLE requires a second piece";
        }

        if (string.IsNullOrWhiteSpace(resultName))
        {
            return $"ASSEMBLE {firstPieceName} {secondPieceName}";
        }

        return $"ASSEMBLE {resultName} {firstPieceName} {secondPieceName}";
    }

    public string GetUnnamedAssemblyName(params string[] pieces)
    {
        if (pieces.Length == 0)
        {
            return "";
        }

        return $"[{string.Join(", ", pieces)}]";
    }
}