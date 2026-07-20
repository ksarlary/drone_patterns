namespace DronePatterns.Utils;

public static class DataPaths
{
    private const string DataDirectoryName = "data";

    private static readonly string DataDirectory = ResolveDataDirectory();

    public static string Stocks =>
        Path.Combine(DataDirectory, "stocks.json");

    public static string DroneCatalog =>
        Path.Combine(DataDirectory, "drone_catalog.json");

    public static string PieceCatalog =>
        Path.Combine(DataDirectory, "piece_catalog.json");

    public static string Orders =>
        Path.Combine(DataDirectory, "orders.json");

    public static string Movements =>
        Path.Combine(DataDirectory, "movements.json");

    private static string ResolveDataDirectory()
    {
        string workingDirectoryDataPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            DataDirectoryName
        );

        if (Directory.Exists(workingDirectoryDataPath))
        {
            return Path.GetFullPath(workingDirectoryDataPath);
        }

        string applicationDataPath = Path.Combine(
            AppContext.BaseDirectory,
            DataDirectoryName
        );

        return Path.GetFullPath(applicationDataPath);
    }
}