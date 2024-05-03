using Hoi4ScriptParser.Model;
using LocalUtilities.StringUtilities;
using System.Text;

namespace Hoi4ScriptParser.Data;

public class ValueArray(Token? from, Word name, uint level) : Token(from, name, level)
{
    public List<List<Word>> Value { get; } = [];

    public void Append(Word value)
    {
        Value.LastOrDefault()?.Add(value);
    }

    public void AppendNew(Word value)
    {
        Value.Add([value]);
    }

    public override string ValueToString()
    {
        return new StringBuilder()
            .AppendJoin('\0', Value, (sb, value) => sb
                .Append('(')
                .AppendJoin(' ', value)
                .Append(')')
                .Append('\n'))
            .ToString();
    }
}