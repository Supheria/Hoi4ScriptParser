using System.Text;

namespace Parser.Data.TokenTypes;

public class Scope : Token
{
    public List<Token> Property { get; }

    public Scope(Token? from, Word name, uint level) : base(from, name, level)
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