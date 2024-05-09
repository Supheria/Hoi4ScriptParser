using System.Text;
using Hoi4ScriptParser.Model;
using LocalUtilities.StringUtilities;

namespace Hoi4ScriptParser.Data;

public class TagValues(Token? from, string name, int level, string @operator, string tag) : Token(from, name, level)
{
    public string Operator { get; } = @operator;

    public string Tag { get; } = tag;

    public List<string> Value { get; } = [];

    public void Append(string value)
    {
        Value.Add(value);
    }

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTagValues(Level, Name, Tag, Value)
            .ToString();
    }
}