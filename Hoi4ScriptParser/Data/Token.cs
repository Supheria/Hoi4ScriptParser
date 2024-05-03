using Hoi4ScriptParser.Model;

namespace Hoi4ScriptParser.Data;

public class Token(Token? from, Word name, uint level)
{
    public Token? From { get; } = from is NullToken ? null : from;

    public Word Name { get; } = name;

    public uint Level { get; } = level;

    public virtual string ValueToString()
    {
        return "";
    }

    public override string ToString()
    {
        return $"{Name.Text} ({Level})";
    }
}