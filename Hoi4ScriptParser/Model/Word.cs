using LocalUtilities.MathBundle;

namespace Hoi4ScriptParser.Model;

public class Word(string text, int line, int column)
{
    public string Text { get; } = text;

    public int Line { get; } = line;

    public int Column { get; } = column;

    public Word() : this("", 0, 0)
    {
    }

    public override string ToString() => $"{Text}({Line},{Column})";

    public static bool operator ==(Word left, Word right) =>
        left.Text == right.Text && left.Line == right.Line && left.Column == right.Column;

    public static bool operator !=(Word left, Word right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is not Word word)
            return false;
        return this == word;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Text, Line, Column);
    }
}