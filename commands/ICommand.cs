namespace DronePatterns.Commands;

public interface ICommand
{
    string Name { get; }

    string Execute(string arguments);
}