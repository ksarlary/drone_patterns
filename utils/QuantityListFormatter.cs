namespace DronePatterns.Utils;

public static class QuantityListFormatter
{
    public static string Format(
        IReadOnlyDictionary<string, int> items
    )
    {
        return string.Join(
            ", ",
            items.Select(
                item => $"{item.Value} {item.Key}"
            )
        );
    }
}