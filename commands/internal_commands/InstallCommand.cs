using System.Text.Json;

namespace DronePatterns.Commands.internal_commands;

public class InstallCommand
{
    public string Execute(string systemName, string pieceName, bool displayOnly = true)
    {
        if (string.IsNullOrWhiteSpace(systemName))
        {
            return "ERROR INSTALL requires a system name";
        }

        if (string.IsNullOrWhiteSpace(pieceName))
        {
            return "ERROR INSTALL requires a piece name";
        }

        return $"INSTALL {systemName} {pieceName}";
    }

    public string GetInstalledPieceName(string systemName, string pieceName)
    {
        if (string.IsNullOrWhiteSpace(systemName) || string.IsNullOrWhiteSpace(pieceName))
        {
            return "";
        }

        return $"{pieceName}{{{systemName}}}";
    }
}