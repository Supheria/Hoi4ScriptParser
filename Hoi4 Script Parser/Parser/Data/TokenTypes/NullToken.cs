namespace Parser.Data.TokenTypes;

public class NullToken : Token
{
    public NullToken() : base(null, new(), 0)
    {
    }
}