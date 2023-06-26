using System.Text;
using Utilities;

namespace Parser.Data.TokenTypes;

public class ValueArray : Token
{
    public List<List<Word>> Value { get; }

    public ValueArray(Word name, uint level) : base(name, level)
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

    public override string ToString()
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