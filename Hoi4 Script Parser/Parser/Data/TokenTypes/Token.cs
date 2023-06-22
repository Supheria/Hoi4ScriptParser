namespace Parser.Data.TokenTypes;

public class Token
{
    public Word Name { get; }

    public uint Level { get; }

    public Token(Word name, uint level)
    {
        Name = name;
        Level = level;
    }

    public override string ToString()
    {
        return Name.Text;
    }

    public uint Line => Name.Line;

    public uint Column => Name.Column;
}