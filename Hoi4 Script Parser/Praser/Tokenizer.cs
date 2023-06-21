using System.Diagnostics.CodeAnalysis;
using Parser.data;
using System.Text;

namespace Parser;

internal class Tokenizer
{
    private static readonly List<char> Delimiter = new() { '\t', ' ', '\n', '\r', '#', '=', '>', '<', '}', '{', '"', '\0' };
    private static readonly List<char> Blank = new() { '\t', ' ', '\n', '\r', '\0' };
    private static readonly List<char> EndLine = new() { '\n', '\r', '\0' };
    private static readonly List<char> Marker = new() { '=', '>', '<', '}', '{' };
    private const char Note = '#';
    private const char Quote = '"';
    private const char Escape = '\\';

    private enum States
    {
        None,
        Quotation,
        Escape,
        Word,
        Note
    }

    private States State { get; set; } = States.None;
    private byte[] Buffer { get; set; } = Array.Empty<byte>();
    private uint BufferPosition { get; set; } = 0;
    private uint Line { get; set; } = 1;
    private uint Column { get; set; } = 0;
    private ParseTree Tree { get; set; }
    private Element Composed { get; set; } = new();
    private StringBuilder Composing { get; } = new();
    private List<Token> Tokens { get; }

    public Tokenizer(string filePath, List<Token> tokens)
    {
        Tokens = tokens;
        ReadBuffer(filePath);
        Tree = new ParseTree();
        while (BufferPosition < Buffer?.Length)
        {
            if (!Compose((char)Buffer[BufferPosition]))
                continue;
            var tree = Tree.Parse(Composed);
            if (tree == null)
            {
                CacheList();
                Tree = new ParseTree();
            }
            else { Tree = tree; }
        }
        EndCheck();
    }
    
    private void ReadBuffer(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Exceptions.Exception($"could not open file: {filePath}");
            return;
        }
        using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        if (file.ReadByte() == 0xEF && file.ReadByte() == 0xBB && file.ReadByte() == 0xBF)
        {
            Buffer = new byte[file.Length - 3];
            _ = file.Read(Buffer, 0, Buffer.Length);
        }
        else
        {
            file.Seek(0, SeekOrigin.Begin);
            Buffer = new byte[file.Length];
            _ = file.Read(Buffer, 0, Buffer.Length);
        }
    }
    private bool Compose(char ch)
    {
        if (Composed.Submitted == false) { return true; }
        switch (State)
        {
            case States.Quotation:
                if (ch == Escape)
                {
                    Composing.Append(GetChar());
                    State = States.Escape;
                    return false;
                }
                else if (ch == Quote)
                {
                    Composing.Append(GetChar());
                    Composed = new(Composing.ToString(), Line, Column);
                    State = States.None;
                    return true;
                }
                else if (EndLine.Contains(ch))
                {
                    Composing.Append(Quote);
                    Composed = new(Composing.ToString(), Line, Column);
                    State = States.None;
                    return true;
                }
                Composing.Append(GetChar());
                return false;
            case States.Escape:
                if (EndLine.Contains(ch))
                {
                    Composing.Append(Quote);
                    Composing.Append(Quote);
                    Composed = new(Composing.ToString(), Line, Column);
                    State = States.None;
                    return true;
                }
                else
                {
                    Composing.Append(GetChar());
                    State = States.Quotation;
                    return false;
                }
            case States.Word:
                if (Delimiter.Contains(ch))
                {
                    Composed = new(Composing.ToString(), Line, Column);
                    State = States.None;
                    return true;
                }
                Composing.Append(GetChar());
                return false;
            case States.Note:
                if (EndLine.Contains(ch))
                {
                    State = States.None;
                }
                GetChar();
                return false;
            default:
                if (ch == Quote)
                {
                    Composing.Clear();
                    Composing.Append(GetChar());
                    State = States.Quotation;
                }
                else if (ch == Note)
                {
                    State = States.Note;
                    GetChar();
                }
                else if (Marker.Contains(ch))
                {
                    Composed = new(GetChar().ToString(), Line, Column);
                    return true;
                }
                else if (Blank.Contains(ch))
                {
                    if (ch == '\n')
                    {
                        Line++;
                        Column = 0;
                    }
                    GetChar();
                }
                else
                {
                    Composing.Clear();
                    Composing.Append(GetChar());
                    State = States.Word;
                }
                return false;
        }
    }
    private void CacheList()
    {
        var token = Tree.OnceGet();
        if (token is NullToken) { return; }
        Tokens.Add(token);
    }

    private char GetChar()
    {
        var ch = (char)Buffer[BufferPosition++];
        if (ch == '\t')
            Column += 4;
        else
            Column++;
        return ch;
    }
    private void EndCheck()
    {
        if (Tree.From != null)
        {
            Exceptions.Exception($"interruption at line({Line}), column({Column})");
            Tree.From.Append(Tree.OnceGet());
            Tree = Tree.From;
            while (Tree.From != null)
            {
                Tree.From.Append(Tree.OnceGet());
                Tree = Tree.From;
            }
        }
        CacheList();
    }
}