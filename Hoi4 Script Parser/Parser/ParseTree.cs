using Parser.Data;
using Parser.Data.TokenTypes;

namespace Parser;

internal class ParseTree
{
    private const char OpenBrace = '{';
    private const char CloseBrace = '}';
    private const char Equal = '=';
    private const char Greater = '>';
    private const char Less = '<';

    private readonly Exceptions _exceptions;

    [Flags]
    private enum Steps
    {
        None = 0,
        Name = 1,
        Operator = 1 << 1,
        Value = 1 << 2,
        Tag = 1 << 3,
        Array = 1 << 4,
        Sub = 1 << 5,
        On = 1 << 6,
        Off = 1 << 7
    }

    private Steps Step { get; set; } = Steps.None;

    private Word Name { get; set; } = new();

    private Word Operator { get; set; } = new();

    private Word Value { get; set; } = new();

    private Word Array { get; set; } = new();

    private Token Builder { get; set; } = new NullToken();

    public ParseTree? From { get; }

    private uint Level { get; }

    public ParseTree(Exceptions exceptions)
    {
        From = null;
        Level = 0;
        _exceptions = exceptions;
    }

    public ParseTree(ParseTree from, uint level, Word key, Word @operator, Exceptions exceptions)
    {
        From = from;
        Level = level;
        Name = key;
        Operator = @operator;
        Step = Steps.Operator;
        _exceptions = exceptions;
    }

    public Token OnceGet()
    {
        if (Builder is NullToken)
            return Builder;
        var builder = Builder;
        Builder = new NullToken();
        return builder;
    }

    public void Done()
    {
        if (From is null)
            return;
        From.Append(Builder);
        Builder = new NullToken();
    }

    private void Append(Token token)
    {
        (Builder as Scope)?.Append(token, _exceptions.ErrorString);
    }

    public ParseTree? Parse(Element element)
    {
        var ch = element.Head();
        if (Step.HasFlag(Steps.Sub))
        {
            return ParseSub(element);
        }
        else if (Step.HasFlag(Steps.Array))
        {
            return ParseArray(element);
        }
        else if (Step.HasFlag(Steps.Operator))
        {
            return ParseOperator(element);
        }
        //
        // 1
        //
        else if (Step.HasFlag(Steps.Name))
        {
            switch (ch)
            {
                case Equal:
                case Greater:
                case Less:
                    Step = Steps.Operator;
                    Operator = element.Get();
                    return this;
                default:
                    _exceptions.UnexpectedOperator(element);
                    element.Get();
                    return From;
            }
        }
        //
        // 3
        //
        else if (Step.HasFlag(Steps.Value))
        {
            switch (ch)
            {
                case OpenBrace:
                    Step = Steps.Tag;
                    element.Get();
                    return this;
                default:
                    Done();
                    // element.Get(); // leave element to next tree
                    return From;
            }
        }
        //
        // 4
        //
        else if (Step.HasFlag(Steps.Tag))
        {
            switch (ch)
            {
                case OpenBrace:
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    element.Get();
                    Done();
                    return From;
                default:
                    Step = Steps.Tag;
                    ((TaggedValue)Builder).Append(element.Get());
                    return this;
            }
        }
        //
        // 0 - None
        //
        else
        {
            switch (ch)
            {
                case OpenBrace:
                case CloseBrace:
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedName(element);
                    element.Get();
                    return From;
                default:
                    Step = Steps.Name;
                    Name = element.Get();
                    return this;
            }
        }
    }

    public ParseTree? ParseSub(Element element)
    {
        var ch = element.Head();
        //
        // 6
        //
        if (Step.HasFlag(Steps.Name))
        {
            switch (ch)
            {
                case OpenBrace:
                    _exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    ((Scope)Builder).Append(
                        new(From?.Builder, Value, Level + 1), _exceptions.ErrorString);
                    element.Get();
                    Done();
                    return From;
                case Equal:
                case Greater:
                case Less:
                    Step = Steps.Sub;
                    return new(this, Level + 1, Value, element.Get(), _exceptions);
                default:
                    ((Scope)Builder).Append(new(From?.Builder, Value, Level + 1), _exceptions.ErrorString);
                    Value = element.Get();
                    return this;
            }
        }
        //
        // 7 - Sub
        //
        else
        {
            switch (ch)
            {
                case OpenBrace:
                case CloseBrace:
                case Equal:
                case Greater:
                case Less:
                    element.Get();
                    Done();
                    return From;
                default:
                    Step = Steps.Sub | Steps.Name;
                    Value = element.Get();
                    return this;
            }
        }
    }

    public ParseTree? ParseArray(Element element)
    {
        var ch = element.Head();
        if (Step.HasFlag(Steps.Tag))
        {
            return ParseTagArray(element);
        }
        else if (Step.HasFlag(Steps.Value))
        {
            return ParseValueArray(element);
        }
        //
        // 9
        //
        else if (Step.HasFlag(Steps.Off))
        {
            switch (ch)
            {
                case OpenBrace:
                    Step = Steps.Array;
                    element.Get();
                    return this;
                case CloseBrace:
                    element.Get();
                    Done();
                    return From;
                default:
                    _exceptions.UnexpectedArraySyntax(element);
                    element.Get();
                    return From;
            }
        }
        //
        // 10
        //
        else if (Step.HasFlag(Steps.Name))
        {
            switch (ch)
            {
                case OpenBrace:
                    _exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case Greater:
                case Less:
                    _exceptions.UnexpectedOperator(element);
                    element.Get();
                    return From;
                case Equal:
                    Step = Steps.Array | Steps.Tag;
                    Builder = new TagArray(From?.Builder, Name, Level);
                    ((TagArray)Builder).AppendNew(Array);
                    element.Get();
                    return this;
                case CloseBrace:
                    Step = Steps.Array | Steps.Value | Steps.Off;
                    Builder = new ValueArray(From?.Builder, Name, Level);
                    ((ValueArray)Builder).AppendNew(Array);
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Value;
                    Builder = new ValueArray(From?.Builder, Name, Level);
                    ((ValueArray)Builder).AppendNew(Array);
                    ((ValueArray)Builder).Append(element.Get());
                    return this;
            }
        }
        //
        // 8 - Array
        //
        else
        {
            switch (ch)
            {
                case OpenBrace:
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Off;
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Name;
                    Array = element.Get();
                    return this;
            }
        }
    }

    public ParseTree? ParseTagArray(Element element)
    {
        var ch = element.Head();
        if (Step.HasFlag(Steps.Value))
        {
            //
            // 17
            //
            if (Step.HasFlag(Steps.Off))
            {
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        _exceptions.UnexpectedName(element);
                        element.Get();
                        return From;
                    case CloseBrace:
                        Step = Steps.Array | Steps.Tag | Steps.Off;
                        element.Get();
                        return this;
                    default:
                        Step = Steps.Array | Steps.Tag | Steps.Name;
                        ((TagArray)Builder).AppendTag(element.Get());
                        return this;
                }
            }
            //
            // 16 - Array | Tag | Value
            //
            else
            {
                switch (ch)
                {
                    case OpenBrace:
                    case Equal:
                    case Greater:
                    case Less:
                        _exceptions.UnexpectedValue(element);
                        element.Get();
                        return From;
                    case CloseBrace:
                        Step = Steps.Array | Steps.Tag | Steps.Value | Steps.Off;
                        element.Get();
                        return this;
                    default:
                        element.Get();
                        ((TagArray)Builder).Append(element.Get());
                        return this;
                }
            }
        }
        //
        // 18
        //
        else if (Step.HasFlag(Steps.Off))
        {
            switch (ch)
            {
                case OpenBrace:
                    Step = Steps.Array | Steps.Tag | Steps.On;
                    element.Get();
                    return this;
                case CloseBrace:
                    element.Get();
                    Done();
                    return From;
                default:
                    _exceptions.UnexpectedArraySyntax(element);
                    element.Get();
                    return From;
            }
        }
        //
        // 19
        //
        else if (Step.HasFlag(Steps.On))
        {
            switch (ch)
            {
                case OpenBrace:
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedName(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Tag | Steps.Off;
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Tag | Steps.Name;
                    ((TagArray)Builder).AppendNew(element.Get());
                    return this;
            }
        }
        //
        // 20
        //
        else if (Step.HasFlag(Steps.Name))
        {
            switch (ch)
            {
                case Equal:
                    Step = Steps.Array | Steps.Tag;
                    element.Get();
                    return this;
                case Greater:
                case Less:
                    _exceptions.UnexpectedOperator(element);
                    element.Get();
                    return From;
                default:
                    _exceptions.UnexpectedArrayType(element);
                    element.Get();
                    return From;
            }
        }
        //
        //
        // 15 - Array | Tag
        else
        {
            switch (ch)
            {
                case OpenBrace:
                    Step = Steps.Array | Steps.Tag | Steps.Value;
                    element.Get();
                    return this;
                default:
                    _exceptions.UnexpectedArrayType(element);
                    element.Get();
                    return From;
            }
        }
    }

    public ParseTree? ParseValueArray(Element element)
    {
        var ch = element.Head();
        //
        // 12
        //
        if (Step.HasFlag(Steps.Off))
        {
            switch (ch)
            {
                case OpenBrace:
                    Step = Steps.Array | Steps.Value | Steps.On;
                    element.Get();
                    return this;
                case CloseBrace:
                    element.Get();
                    Done();
                    return From;
                default:
                    _exceptions.UnexpectedArraySyntax(element);
                    element.Get();
                    return From;
            }
        }
        //
        // 13
        //
        else if (Step.HasFlag(Steps.On))
        {
            switch (ch)
            {
                case OpenBrace:
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Value | Steps.Off;
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Value | Steps.Name;
                    ((ValueArray)Builder).AppendNew(element.Get());
                    return this;
            }
        }
        //
        // 14
        //
        else if (Step.HasFlag(Steps.Name))
        {
            switch (ch)
            {
                case OpenBrace:
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedArrayType(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Value | Steps.Off;
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Value;
                    ((ValueArray)Builder).Append(element.Get());
                    return this;
            }
        }
        //
        // 11 - Array | Value
        //
        else
        {
            switch (ch)
            {
                case OpenBrace:
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Value | Steps.Off;
                    element.Get();
                    return this;
                default:
                    ((ValueArray)Builder).Append(element.Get());
                    return this;
            }
        }
    }

    public ParseTree? ParseOperator(Element element)
    {
        var ch = element.Head();
        //
        // 5
        //
        if (Step.HasFlag(Steps.On))
        {
            switch (ch)
            {
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    element.Get();
                    Done();
                    return From;
                case OpenBrace:
                    Step = Steps.Array;
                    element.Get();
                    return this;
                default:
                    Step = Steps.Sub | Steps.Name;
                    Value = element.Get();
                    Builder = new Scope(From?.Builder, Name, Level);
                    return this;
            }
        }
        //
        // 2 - Op
        //
        else
        {
            switch (ch)
            {
                case CloseBrace:
                case Equal:
                case Greater:
                case Less:
                    _exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case OpenBrace:
                    if (Operator.Text[0] != Equal)
                    {
                        _exceptions.UnexpectedOperator(element);
                        element.Get();
                        return From;
                    }
                    else
                    {
                        Step = Steps.Operator | Steps.On;
                        element.Get();
                        return this;
                    }
                default:
                    Step = Steps.Value;
                    Builder = new TaggedValue(From?.Builder, Name, Level, Operator, element.Get());
                    return this;
            }
        }
    }
}