namespace Parser.data;

internal class Element
{
    private string Text { get; }
    public bool Submitted { get; private set; }
    public uint Line { get; private set; }
    public uint Column { get; private set; }

    public Element()
    {
        Submitted = true;
        Text = "";
        Line = 0;
        Column = 0;
    }
    public Element(string text, uint line, uint column)
    {
        Submitted = false;
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