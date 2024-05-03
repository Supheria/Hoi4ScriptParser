using Hoi4ScriptParser.Data;

namespace Hoi4ScriptParser;

public class TokenInfo(Token token, string filePath)
{
    public Token Token { get; init; } = token;

    public string FilePath { get; init; } = filePath;
}