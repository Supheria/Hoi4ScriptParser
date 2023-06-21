namespace Parser.data;

public class Word
{
    public string Text { get; init; }
    public uint Line { get; init; }
    public uint Column { get; init; }

    public Word(string text, uint line, uint column)
    {
        Text = text;
        Line = line;
        Column = column;
    }

    public Word()
    {
        Text = "";
        Line = 0;
        Column = 0;
    }

    public override string ToString()
    {
        return Text;
    }
}