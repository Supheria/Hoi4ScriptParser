namespace Parser.Data;

public class Word
{
    public string Text { get; }

    public uint Line { get; }

    public uint Column { get; }

    public Word(string text, uint line, uint column)
    {
        Text = text;
        Line = line;
        Column = column;
    }

    public Word() : this("", 0, 0)
    {
    }

    public override string ToString()
    {
        return Text;
    }
}