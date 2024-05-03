using System.Text;
using Hoi4ScriptParser.Model;

namespace Hoi4ScriptParser.Data;

public class Scope(Token? from, Word name, uint level) : Token(from, name, level)
{
    public List<Token> Property { get; } = [];

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