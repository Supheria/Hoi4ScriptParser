using System.Text;
using Hoi4ScriptParser.Model;

namespace Hoi4ScriptParser.Data;

public class TaggedValue(Token? from, Word name, uint level, Word @operator, Word tag) : Token(from, name, level)
{
    public Word Operator { get; } = @operator;

    public Word Tag { get; } = tag;

    public List<Word> Value { get; } = new();

    public void Append(Word value)
    {
        Value.Add(value);
    }

    public override string ValueToString()
    {
        return new StringBuilder()
            .Append(Tag)
            .Append('[')
            .AppendJoin(' ', Value)
            .Append(']')
            .ToString();
    }
}