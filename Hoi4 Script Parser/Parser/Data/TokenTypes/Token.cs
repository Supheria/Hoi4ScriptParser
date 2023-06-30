namespace Parser.Data.TokenTypes;

public class Token
{
    public Token? From { get; }

    public Word Name { get; }

    public uint Level { get; }

    public Token(Token? from, Word name, uint level)
    {
        From = from is NullToken ? null : from;
        Name = name;
        Level = level;
    }

    public virtual string ValueToString()
    {
        return "";
    }

    public override string ToString()
    {
        return $"{Name.Text} ({Level})";
    }

    public uint Line => Name.Line;

    public uint Column => Name.Column;
}