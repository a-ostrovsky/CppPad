namespace CppPad.LspClient.Model;

/// <summary>
/// Represents a range with an inclusive start and an exclusive end position.
/// </summary>
/// <param name="Start">Inclusive start</param>
/// <param name="End">Exclusive end</param>
public record Range(Position Start, Position End);