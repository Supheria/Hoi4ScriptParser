﻿using Hoi4ScriptParser.Data;

namespace Hoi4ScriptParser.Model;

internal class ParseTree
{
    const char OpenBrace = '{';
    const char CloseBrace = '}';
    const char Equal = '=';
    const char Greater = '>';
    const char Less = '<';

    Exceptions Exceptions { get; }

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

    private string Name { get; set; } = "";

    private string Operator { get; set; } = "";

    private string Value { get; set; } = "";

    private string Array { get; set; } = "";

    private Token Builder { get; set; } = new NullToken();

    public ParseTree? From { get; }

    private int Level { get; }

    public ParseTree(Exceptions exceptions)
    {
        From = null;
        Level = 0;
        Exceptions = exceptions;
    }

    public ParseTree(ParseTree from, int level, string key, string @operator, Exceptions exceptions)
    {
        From = from;
        Level = level;
        Name = key;
        Operator = @operator;
        Step = Steps.Operator;
        Exceptions = exceptions;
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
        (Builder as Scope)?.Append(token, Exceptions.Log);
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
                    Exceptions.UnexpectedOperator(element);
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
                    Exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    element.Get();
                    Done();
                    return From;
                default:
                    Step = Steps.Tag;
                    ((TagValues)Builder).Append(element.Get());
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
                    Exceptions.UnexpectedName(element);
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
                    Exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    ((Scope)Builder).Append(
                        new(From?.Builder, Value, Level + 1), Exceptions.Log);
                    element.Get();
                    Done();
                    return From;
                case Equal:
                case Greater:
                case Less:
                    Step = Steps.Sub;
                    return new(this, Level + 1, Value, element.Get(), Exceptions);
                default:
                    ((Scope)Builder).Append(new(From?.Builder, Value, Level + 1), Exceptions.Log);
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
                    Exceptions.UnexpectedArraySyntax(element);
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
                    Exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case Greater:
                case Less:
                    Exceptions.UnexpectedOperator(element);
                    element.Get();
                    return From;
                case Equal:
                    Step = Steps.Array | Steps.Tag;
                    Builder = new TagValuesPairsArray(From?.Builder, Name, Level);
                    ((TagValuesPairsArray)Builder).AppendNew(Array);
                    element.Get();
                    return this;
                case CloseBrace:
                    Step = Steps.Array | Steps.Value | Steps.Off;
                    Builder = new ValuesArray(From?.Builder, Name, Level);
                    ((ValuesArray)Builder).AppendNew(Array);
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Value;
                    Builder = new ValuesArray(From?.Builder, Name, Level);
                    ((ValuesArray)Builder).AppendNew(Array);
                    ((ValuesArray)Builder).Append(element.Get());
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
                    Exceptions.UnexpectedValue(element);
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
                        Exceptions.UnexpectedName(element);
                        element.Get();
                        return From;
                    case CloseBrace:
                        Step = Steps.Array | Steps.Tag | Steps.Off;
                        element.Get();
                        return this;
                    default:
                        Step = Steps.Array | Steps.Tag | Steps.Name;
                        ((TagValuesPairsArray)Builder).AppendTag(element.Get());
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
                        Exceptions.UnexpectedValue(element);
                        element.Get();
                        return From;
                    case CloseBrace:
                        Step = Steps.Array | Steps.Tag | Steps.Value | Steps.Off;
                        element.Get();
                        return this;
                    default:
                        element.Get();
                        ((TagValuesPairsArray)Builder).Append(element.Get());
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
                    Exceptions.UnexpectedArraySyntax(element);
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
                    Exceptions.UnexpectedName(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Tag | Steps.Off;
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Tag | Steps.Name;
                    ((TagValuesPairsArray)Builder).AppendNew(element.Get());
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
                    Exceptions.UnexpectedOperator(element);
                    element.Get();
                    return From;
                default:
                    Exceptions.UnexpectedArrayType(element);
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
                    Exceptions.UnexpectedArrayType(element);
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
                    Exceptions.UnexpectedArraySyntax(element);
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
                    Exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Value | Steps.Off;
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Value | Steps.Name;
                    ((ValuesArray)Builder).AppendNew(element.Get());
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
                    Exceptions.UnexpectedArrayType(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Value | Steps.Off;
                    element.Get();
                    return this;
                default:
                    Step = Steps.Array | Steps.Value;
                    ((ValuesArray)Builder).Append(element.Get());
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
                    Exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case CloseBrace:
                    Step = Steps.Array | Steps.Value | Steps.Off;
                    element.Get();
                    return this;
                default:
                    ((ValuesArray)Builder).Append(element.Get());
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
                    Exceptions.UnexpectedValue(element);
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
                    Exceptions.UnexpectedValue(element);
                    element.Get();
                    return From;
                case OpenBrace:
                    if (Operator[0] != Equal)
                    {
                        Exceptions.UnexpectedOperator(element);
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
                    Builder = new TagValues(From?.Builder, Name, Level, Operator, element.Get());
                    return this;
            }
        }
    }
}