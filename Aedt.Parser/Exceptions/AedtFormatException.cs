namespace Aedt.Parser.Exceptions;

/// <summary>
/// Thrown when a line of an AEDT CSV file does not have the expected fields.
/// </summary>
public sealed class AedtFormatException : FormatException
{
    /// <summary>
    /// Creates an exception for a malformed line.
    /// </summary>
    /// <param name="filePath">File being read.</param>
    /// <param name="lineNumber">1-based line number of the malformed line.</param>
    /// <param name="line">Content of the malformed line.</param>
    /// <param name="reason">Why the line is rejected.</param>
    public AedtFormatException(string filePath, int lineNumber, string line, string reason)
        : base($"{filePath}, line {lineNumber}: {reason} Line: \"{line}\"")
    {
        FilePath = filePath;
        LineNumber = lineNumber;
        Line = line;
    }

    /// <summary>
    /// File being read.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// 1-based line number of the malformed line.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Content of the malformed line.
    /// </summary>
    public string Line { get; }
}
