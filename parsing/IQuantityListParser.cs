using DronePatterns.Models;

namespace DronePatterns.Parsing;

public interface IQuantityListParser
{
    QuantityListParseResult Parse(string arguments);
}