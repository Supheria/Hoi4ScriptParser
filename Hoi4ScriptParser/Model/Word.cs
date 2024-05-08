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

    public override string ToString() => $"{Text}";

    public static bool operator ==(Word word, object? obj)
    {
        if (obj is not Word other)
            return false;
        return word.Text == other.Text && word.Line == other.Line && word.Column == other.Column;
    }

    public static bool operator !=(Word word, object? obj)
    {
        return !(word == obj);
    }

    public override bool Equals(object? obj)
    {
        return this == obj;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Text, Line, Column);
    }
}