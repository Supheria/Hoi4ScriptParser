using Hoi4ScriptParser.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text;
using LocalUtilities.StringUtilities;

namespace Hoi4ScriptParser.Data;

public class Token(Token? from, string name, int level)
{
    public Token? From { get; } = from is NullToken ? null : from;

    public string Name { get; } = name;

    public int Level { get; } = level;

    public override string ToString()
    {
        return new StringBuilder()
            .AppendTab(Level)
            .Append($"{Name}\n")
            .ToString();
    }
}