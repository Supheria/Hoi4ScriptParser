using System.Text;
using Parser.Utilities;

namespace Parser.Data.TokenTypes;

public class ValueArray : Token
{
    public List<List<Word>> Value { get; }

    public ValueArray(Token? from, Word name, uint level) : base(from, name, level)
    {
        Value = new();
    }

    public void Append(Word value)
    {
        Value.LastOrDefault()?.Add(value);
    }

    public void AppendNew(Word value)
    {
        Value.Add(new() { value });
    }

    public override string ValueToString()
    {
        return new StringBuilder()
            .AppendJoinExt('\0', Value, (sb, value) => sb
                .Append('(')
                .AppendJoin(' ', value)
                .Append(')')
                .Append('\n'))
            .ToString();
    }
}