namespace DronePatterns.Commands;

public class AddTemplateCommand : ICommand
{
	public string Name => "ADD_TEMPLATE";

	public string Execute(string arguments)
	{
		if (string.IsNullOrWhiteSpace(arguments))
		{
			return "ERROR Invalid template format";
		}

		string[] parts = arguments.Split(',', StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 7)
		{
			return "ERROR Invalid template format";
		}

		string templateName = parts[0].Trim();

		if (string.IsNullOrWhiteSpace(templateName))
		{
			return "ERROR Invalid template format";
		}

		return $"TEMPLATE_FORMAT_VALID {templateName}";
	}
}