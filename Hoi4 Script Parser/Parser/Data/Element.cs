namespace Parser.Data;

internal class Element
{
    private string Text { get; }

    public bool Submitted { get; private set; }

    public uint Line { get; }

    public uint Column { get; }

    public Element()
    {
        Text = "";
        Submitted = true;
    }

    public Element(string text, uint line, uint column)
    {
        Text = text;
        Line = line;
        Column = column;
    }

    public char Head()
    {
        return Text.FirstOrDefault();
    }

    public Word Get()
    {
        Submitted = true;
        return new(Text, Line, Column);
    }
}