using Hoi4ScriptParser.Model;
using LocalUtilities.StringUtilities;
using System.Text;

namespace Hoi4ScriptParser.Data;

public class TagArray(Token? from, Word name, int level) : Token(from, name, level)
{
    public List<List<KeyValuePair<Word, List<Word>>>> Value { get; } = [];

    public void Append(Word value)
    {
        Value.LastOrDefault()?.LastOrDefault().Value.Add(value);
    }

    public void AppendTag(Word value)
    {
        Value.LastOrDefault()?.Add(new(value, []));
    }

    public void AppendNew(Word value)
    {
        Value.Add([new(value, [])]);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTab(Level)
            .Append($"{Name} = {{\n")
            .AppendJoin('\0', Value, (sb, pairs) =>
            {
                sb.AppendTab(Level + 1)
                .Append("{ ")
                .AppendJoin(' ', pairs, (sb, pair) =>
                {
                    sb.Append(pair.Key)
                    .Append(" = { ")
                    .AppendJoin(' ', pair.Value)
                    .Append(" }");
                })
                .Append(" }\n");
            })
            .AppendTab(Level)
            .Append("}\n")
            .ToString();
    }
}