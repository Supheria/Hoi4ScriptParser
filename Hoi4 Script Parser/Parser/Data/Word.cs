namespace Parser.Data;

public class Word
{
    public string Text { get; }

    public TextPosition Position { get; }

    public uint Line => Position.Line;

    public uint Column => Position.Column;

    public Word(string text, uint line, uint column)
    {
        Text = text;
        Position = new(line, column - (uint)text.Length);
    }

    public Word() : this("", 0, 0)
    {
    }

    public override string ToString() => $"{Text}{Position}";

    public static bool operator ==(Word left, Word right) =>
        left.Text == right.Text && left.Line == right.Line && left.Column == right.Column;

    public static bool operator !=(Word left, Word right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is not Word word)
            return false;
        return this == word;
    }

    public override int GetHashCode() => (Text, Position).GetHashCode();
}