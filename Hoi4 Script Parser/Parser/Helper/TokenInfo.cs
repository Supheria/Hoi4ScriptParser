using Parser.Data.TokenTypes;

namespace Parser.Helper;

public class TokenInfo
{
    public Token Token { get; init; }

    public string FilePath { get; init; }

    public TokenInfo(Token token, string filePath)
    {
        Token = token;
        FilePath = filePath;
    }
}