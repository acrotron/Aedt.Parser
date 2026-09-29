using System.Globalization;
using Aedt.Parser.Exceptions;

namespace Aedt.Parser;

/// <summary>
/// Splits and parses the lines of AEDT CSV exports: comma-separated numbers (optionally with spaces after the commas),
/// ending with an <c>END</c> (or <c>END,,</c>) trailer line.
/// </summary>
internal static class AedtCsvLine
{
    /// <summary>
    /// Returns true for lines that carry no data: blank lines and the <c>END</c> trailer.
    /// </summary>
    internal static bool IsSkipped(string line)
    {
        string trimmed = line.Trim();
        return trimmed.Length == 0 || trimmed.StartsWith("END", StringComparison.Ordinal);
    }

    /// <summary>
    /// Splits a line into its fields. Empty fields are kept, so they cannot shift later values into the wrong column.
    /// </summary>
    internal static string[] Split(string line) => line.Split(',');

    /// <summary>
    /// Parses a field as an invariant-culture number.
    /// </summary>
    /// <exception cref="AedtFormatException">The field is empty or not a number.</exception>
    internal static double ParseDouble(string field, string filePath, int lineNumber, string line)
    {
        if (!double.TryParse(field, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new AedtFormatException(filePath, lineNumber, line, $"\"{field.Trim()}\" is not a number.");
        }

        return value;
    }
}
