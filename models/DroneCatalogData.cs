namespace DronePatterns.Models;

public class DroneCatalogData
{
    public Dictionary<string, DroneDefinition> Drones { get; set; } = new();
}