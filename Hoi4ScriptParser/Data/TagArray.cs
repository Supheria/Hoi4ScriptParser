using Hoi4ScriptParser.Model;
using LocalUtilities.StringUtilities;
using System.Text;

namespace Hoi4ScriptParser.Data;

public class TagArray(Token? from, Word name, uint level) : Token(from, name, level)
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

    public override string ValueToString()
    {
        return new StringBuilder()
            .AppendJoin('\0', Value, (sb, value) => sb
                    .Append('(').AppendJoin(' ', value, (sb2, pair) => sb2
                    .Append(pair.Key)
                    .Append('[')
                    .AppendJoin(' ', pair.Value)
                    .Append(']'))
                .Append(')')
                .Append('\n'))
            .ToString();
    }
}