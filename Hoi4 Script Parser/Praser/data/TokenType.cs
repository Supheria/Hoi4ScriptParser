using System.Text;

namespace Parser.data;

public class NullToken : Token
{
    public NullToken()
        : base(new(), 0)
    {
    }
}

public class Token
{
    public Word Name { get; init; }
    public uint Level { get; init; }
    public Token(Word name, uint level)
    {
        Name = name;
        Level = level;
    }

    public override string ToString()
    {
        return Name.ToString();
    }

    public uint Line => Name.Line;
    public uint Column => Name.Column;
}

public class Scope : Token
{
    public List<Token> Property { get; }

    public Scope(Word name, uint level) : base(name, level)
    {
        Property = new();
    }

    public void Append(Token property, StringBuilder errorLog)
    {
        if (property is NullToken)
            return;
        if (property.Level != Level + 1)
        {
            errorLog.AppendLine("level mismatched of Appending in Scope");
            return;
        }
        Property.Add(property);
    }
}

public class TaggedValue : Token
{
    public Word Operator { get; init; }
    public Word Tag { get; init; }
    public List<Word> Value { get; }
    public TaggedValue(Word name, uint level, Word @operator, Word tag)
        : base(name, level)
    {
        Operator = @operator;
        Tag = tag;
        Value = new();
    }
    
    public void Append(Word value)
    {
        Value.Add(value);
    }

    public new string ToString()
    {
        var sb = new StringBuilder();
        _ = sb.Append($"{Tag}[");
        foreach (var value in Value)
            _ = sb.Append($"{value} ");
        _ = sb.Append(']');
        return sb.ToString();
    }
}

public class ValueArray : Token
{
    public List<List<Word>> Value { get; }
    public ValueArray(Word name, uint level)
        : base(name, level)
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
    public new string ToString()
    {
        var sb = new StringBuilder();
        foreach (var value in Value)
        {
            _ = sb.Append('(');
            foreach (var element in value)
                _ = sb.Append($"{element} ");
            _ = sb.Append(") ");
        }
        return sb.ToString();
    }
}

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
    public new string ToString()
    {
        var sb = new StringBuilder();
        foreach (var value in Value)
        {
            sb.Append('(');
            foreach (var pair in value)
            {
                _ = sb.Append($"{pair.Key}[");
                foreach (var element in pair.Value)
                    _ = sb.Append($"{element} ");
                _ = sb.Append("] ");
            }
            sb.Append(") ");
        }
        return sb.ToString();
    }
}