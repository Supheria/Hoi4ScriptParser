namespace Parser.Data;

public readonly struct TextPosition : IComparable
{
    public uint Line { get; }

    public uint Column { get; }

    public TextPosition(uint line, uint column)
    {
        Line = line;
        Column = column;
    }

    public TextPosition() : this(0, 0)
    {
    }

    public override string ToString()
    {
        return $"({Line},{Column})";
    }

    public int CompareTo(object? obj)
    {
        if (obj is not TextPosition other)
            return -1;
        var left = (Line, Column);
        var right = (other.Line, other.Column);
        return left.CompareTo(right);
    }

    public override int GetHashCode() => (Line, Column).GetHashCode();
}