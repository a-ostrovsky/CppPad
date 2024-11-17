using System;

namespace CppPad.Gui.ViewModels;

/// <summary>
/// Creates a new instance of the <see cref="GoToLineRequestedEventArgs"/> class.
/// </summary>
/// <param name="line">One based line number</param>
/// <param name="character">Zero based character index</param>
public class GoToLineRequestedEventArgs(int line, int character) : EventArgs
{
    /// <summary>
    /// Gets the one-based line number.
    /// </summary>
    public int Line => line;
    
    /// <summary>
    /// Gets the zero-based character index in the line.
    /// </summary>
    public int Character => character;
}