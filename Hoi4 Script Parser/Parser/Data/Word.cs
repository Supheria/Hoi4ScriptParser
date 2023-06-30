namespace Parser.Data;

public class Word
{
    public string Text { get; }

    public uint Line { get; }

    public uint Column { get; }

    public Word(string text, uint line, uint column)
    {
        Text = text;
        Line = line;
        Column = column - (uint)text.Length;
    }

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