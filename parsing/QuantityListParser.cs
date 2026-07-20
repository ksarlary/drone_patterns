using DronePatterns.Models;

namespace DronePatterns.Parsing;

public class QuantityListParser : IQuantityListParser
{
    public QuantityListParseResult Parse(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return QuantityListParseResult.Failure(
                "ERROR Missing arguments"
            );
        }

        List<string>? parts = SplitTopLevelArguments(arguments);

        if (parts == null || parts.Count == 0)
        {
            return QuantityListParseResult.Failure(
                "ERROR Invalid argument format"
            );
        }

        Dictionary<string, int> items = new(
            StringComparer.Ordinal
        );

        foreach (string part in parts)
        {
            QuantityListParseResult itemResult =
                ParseSingleItem(part);

            if (!itemResult.IsSuccess)
            {
                return itemResult;
            }

            KeyValuePair<string, int> item =
                itemResult.Items.Single();

            if (items.TryGetValue(
                    item.Key,
                    out int currentQuantity
                ))
            {
                if (
                    currentQuantity >
                    int.MaxValue - item.Value
                )
                {
                    return QuantityListParseResult.Failure(
                        $"ERROR Quantity is too large for `{item.Key}`"
                    );
                }

                items[item.Key] =
                    currentQuantity + item.Value;
            }
            else
            {
                items.Add(
                    item.Key,
                    item.Value
                );
            }
        }

        return QuantityListParseResult.Success(items);
    }

    private static QuantityListParseResult ParseSingleItem(
        string rawItem
    )
    {
        string cleanedItem = rawItem.Trim();

        if (string.IsNullOrWhiteSpace(cleanedItem))
        {
            return QuantityListParseResult.Failure(
                "ERROR Empty item in argument list"
            );
        }

        int firstWhitespaceIndex =
            FindFirstWhitespaceIndex(cleanedItem);

        if (firstWhitespaceIndex < 0)
        {
            return QuantityListParseResult.Failure(
                $"ERROR Invalid item format `{cleanedItem}`"
            );
        }

        string quantityText = cleanedItem
            [..firstWhitespaceIndex]
            .Trim();

        string itemName = cleanedItem
            [firstWhitespaceIndex..]
            .Trim();

        if (!int.TryParse(
                quantityText,
                out int quantity
            ))
        {
            return QuantityListParseResult.Failure(
                $"ERROR Invalid quantity `{quantityText}`"
            );
        }

        if (quantity <= 0)
        {
            return QuantityListParseResult.Failure(
                $"ERROR Quantity for `{itemName}` must be greater than 0"
            );
        }

        if (string.IsNullOrWhiteSpace(itemName))
        {
            return QuantityListParseResult.Failure(
                "ERROR Missing item name"
            );
        }

        Dictionary<string, int> item = new()
        {
            [itemName] = quantity
        };

        return QuantityListParseResult.Success(item);
    }

    private static int FindFirstWhitespaceIndex(
        string value
    )
    {
        for (int index = 0; index < value.Length; index++)
        {
            if (char.IsWhiteSpace(value[index]))
            {
                return index;
            }
        }

        return -1;
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
            char currentCharacter = arguments[index];

            if (currentCharacter == '[')
            {
                bracketDepth++;
            }
            else if (currentCharacter == ']')
            {
                bracketDepth--;

                if (bracketDepth < 0)
                {
                    return null;
                }
            }
            else if (
                currentCharacter == ','
                && bracketDepth == 0
            )
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

        string finalPart = arguments[
            partStartIndex..
        ];

        if (string.IsNullOrWhiteSpace(finalPart))
        {
            return null;
        }

        parts.Add(finalPart.Trim());

        return parts;
    }
}