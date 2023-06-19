namespace Parser.data;

internal class Element
{
    private string Value { get; }
    public bool OwnValue { get; private set; }
    public uint Line { get; private set; }
    public uint Column { get; private set; }

    public Element()
    {
        OwnValue = false;
        Value = "";
        Line = 0;
        Column = 0;
    }
    public Element(string value, uint line, uint column)
    {
        OwnValue = true;
        Value = value;
        Line = line;
        Column = column;
    }
    public char Head()
    {
        return Value.FirstOrDefault();
    }
    public string GetValue()
    {
        OwnValue = false;
        return Value;
    }
}