using Hoi4ScriptParser.Model;
using LocalUtilities.StringUtilities;
using System.Text;

namespace Hoi4ScriptParser.Data;

public class ValueArray(Token? from, Word name, int level) : Token(from, name, level)
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

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTab(Level)
            .Append($"{Name} = {{\n")
            .AppendJoin('\0', Value, (sb, value) =>
            {
                sb.AppendTab(Level + 1)
                .Append("{ ")
                .AppendJoin(' ', value)
                .Append(" }\n");
            })
            .AppendTab(Level)
            .Append("}\n")
            .ToString();
    }
}