using System.Text;
using Hoi4ScriptParser.Model;
using LocalUtilities.StringUtilities;

namespace Hoi4ScriptParser.Data;

public class TaggedValue(Token? from, Word name, int level, Word @operator, Word tag) : Token(from, name, level)
{
    public Word Operator { get; } = @operator;

    public Word Tag { get; } = tag;

    public List<Word> Value { get; } = new();

    public void Append(Word value)
    {
        Value.Add(value);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTab(Level)
            .Append($"{Name} = {Tag} {(Value.Count is 0 ? '\0' : "{ ")}")
            .AppendJoin(' ', Value)
            .Append($"{(Value.Count is 0 ? '\0' : " }")}\n")
            .ToString();
    }
}