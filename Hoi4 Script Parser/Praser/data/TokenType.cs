using System.Text;

namespace Parser.data;

public class Token
{
    public string Name { get; init; }
    public uint Level { get; init; }
    public Token(string name, uint level)
    {
        Name = name;
        Level = level;
    }
}

public class NullToken : Token
{
    public NullToken() : base("", 0)
    {
    }
}

public class TaggedValue : Token
{
    public string Operator { get; init; }
    public string Tag { get; init; }
    public List<string> Value { get; private set; }
    public TaggedValue(string name, uint level, string @operator, string tag) : base(name, level)
    {
        Operator = @operator;
        Tag = tag;
        Value = new();
    }
    public void Append(string value)
    {
        Value.Add(value);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        _ = sb.Append($"{Tag}(...) ");
        foreach (var value in Value)
            _ = sb.Append($"{value} ");

        return sb.ToString();
    }
}

public class ValueArray : Token
{
    public List<List<string>> Value { get; private set; }
    public ValueArray(string name, uint level) : base(name, level)
    {
        Value = new();
    }
    public void Append(string value)
    {
        Value.LastOrDefault()?.Add(value);
    }
    public void AppendNew(string value)
    {
        Value.Add(new() { value });
    }
}

public class TagArray : Token
{
    public List<List<KeyValuePair<string, List<string>>>> Value { get; private set; }
    public TagArray(string name, uint level) : base(name, level)
    {
        Value = new();
    }
    public void Append(string value)
    {
        Value.LastOrDefault()?.LastOrDefault().Value.Add(value);
    }
    public void AppendTag(string value)
    {
        Value.LastOrDefault()?.Add(new(value, new()));
    }
    public void AppendNew(string value)
    {
        Value.Add(new() { new(value, new()) });
    }
}

public class Scope : Token
{
    public List<Token> Property { get; private set; }
    public Scope(string name, uint level) : base(name, level)
    {
        Property = new();
    }
    public void Append(Token property)
    {
        if (property is NullToken)
            return;
        if (property.Level != Level + 1)
        {
            Exceptions.Exception("level mismatched of Appending in Scope");
            return;
        }
        Property.Add(property);
    }
}