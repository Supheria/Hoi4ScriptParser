﻿using System.Text;
using Hoi4ScriptParser.Model;
using LocalUtilities.StringUtilities;

namespace Hoi4ScriptParser.Data;

public class Scope(Token? from, string name, int level) : Token(from, name, level)
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

    public override string ToString()
    {
        return new StringBuilder()
            .AppendNameStart(Level, Name)
            .AppendJoin('\0', Property, (sb, property) => 
            {
                sb.Append(property.ToString());
            })
            .AppendNameEnd(Level)
            .ToString();
    }
}