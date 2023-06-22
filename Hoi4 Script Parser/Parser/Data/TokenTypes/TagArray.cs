using System.Text;
using Utilities;

namespace Parser.Data.TokenTypes;

public class TagArray : Token
{
    public List<List<KeyValuePair<Word, List<Word>>>> Value { get; }

    public TagArray(Word name, uint level)
        : base(name, level)
    {
        Value = new();
    }

    public void Append(Word value)
    {
        Value.LastOrDefault()?.LastOrDefault().Value.Add(value);
    }

    public void AppendTag(Word value)
    {
        Value.LastOrDefault()?.Add(new(value, new()));
    }

    public void AppendNew(Word value)
    {
        Value.Add(new() { new(value, new()) });
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendJoinExt(' ', Value, (sb, value) => sb
                .Append('(')
                .AppendJoinExt(' ', value, (sb2, pair) => sb2
                    .Append(pair.Key)
                    .Append('[')
                    .AppendJoin(' ', pair.Value)
                    .Append(']'))
                .Append(')'))
            .ToString();
    }
}