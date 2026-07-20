namespace DronePatterns.Models;

public class DroneDefinition
{
    public string Hull { get; set; } = "";

    public string Core { get; set; } = "";

    public string System { get; set; } = "";

    public List<string> Generators { get; set; } = new();

    public List<string> MovementModules { get; set; } = new();

    public string Processor { get; set; } = "";
}