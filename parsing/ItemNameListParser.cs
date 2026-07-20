using DronePatterns.Models;

namespace DronePatterns.Parsing;

public class ItemNameListParser
{
    public ItemNameListParseResult Parse(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return ItemNameListParseResult.Failure(
                "ERROR Missing item list"
            );
        }

        List<string>? parts = SplitTopLevelArguments(arguments);

        if (parts == null || parts.Count == 0)
        {
            return ItemNameListParseResult.Failure(
                "ERROR Invalid item list"
            );
        }

        List<string> normalizedItems = new();

        foreach (string part in parts)
        {
            string normalizedName = NormalizeItemName(part);

            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return ItemNameListParseResult.Failure(
                    "ERROR Item name cannot be empty"
                );
            }

            if (!normalizedItems.Contains(
                    normalizedName,
                    StringComparer.Ordinal
                ))
            {
                normalizedItems.Add(normalizedName);
            }
        }

        return ItemNameListParseResult.Success(normalizedItems);
    }

    private static string NormalizeItemName(string rawName)
    {
        string name = rawName.Trim();

        if (!name.StartsWith('[') || !name.EndsWith(']'))
        {
            return name;
        }

        string innerContent = name[1..^1];

        string[] components = innerContent.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries
        );

        string[] normalizedComponents = components
            .OrderBy(
                component => component,
                StringComparer.Ordinal
            )
            .ToArray();

        return $"[{string.Join(", ", normalizedComponents)}]";
    }

    private static List<string>? SplitTopLevelArguments(
        string arguments
    )
    {
        List<string> parts = new();
        int bracketDepth = 0;
        int partStartIndex = 0;

        for (int index = 0; index < arguments.Length; index++)
        {
            char character = arguments[index];

            if (character == '[')
            {
                bracketDepth++;
            }
            else if (character == ']')
            {
                bracketDepth--;

                if (bracketDepth < 0)
                {
                    return null;
                }
            }
            else if (character == ',' && bracketDepth == 0)
            {
                string part = arguments[
                    partStartIndex..index
                ];

                if (string.IsNullOrWhiteSpace(part))
                {
                    return null;
                }

                parts.Add(part.Trim());
                partStartIndex = index + 1;
            }
        }

        if (bracketDepth != 0)
        {
            return null;
        }

        string lastPart = arguments[partStartIndex..];

        if (string.IsNullOrWhiteSpace(lastPart))
        {
            return null;
        }

        parts.Add(lastPart.Trim());

        return parts;
    }
}