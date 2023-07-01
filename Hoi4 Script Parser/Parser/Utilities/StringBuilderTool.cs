using System.Text;

namespace Parser.Utilities;

public static class StringBuilderTool
{
    public static StringBuilder AppendJoinExt<T>(this StringBuilder sb, char separator, IList<T> source, Func<StringBuilder, T, StringBuilder> func)
    {
        if (source.Count is 0)
            return sb;

        if (source[0] is not null)
        {
            func(sb, source[0]);
        }

        for (var i = 1; i < source.Count; ++i)
        {
            sb.Append(separator);
            if (source[i] is not null)
            {
                func(sb, source[i]);
            }
        }
        return sb;
    }
}