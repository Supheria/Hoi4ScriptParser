using Parser.data;

namespace Parser.helper;

public class TokenInfo
{
    public Token Token { get; init; }
    public string FilePath { get; init; }

    public TokenInfo(Token token, string filePath)
    {
        Token = token;
        FilePath = filePath;
    }

    public string Name => Token.ToString();
    public uint Level => Token.Level;

    public uint Line => Token.Line;
    public uint Column => Token.Column;
}