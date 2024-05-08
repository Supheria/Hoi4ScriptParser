using Hoi4ScriptParser.Data;
using Hoi4ScriptParser.Model;

namespace Hoi4ScriptParser;

public class Analyzer
{
    Exceptions Exceptions { get; } = new();

    public string ErroLog => Exceptions.Log.ToString();

    public List<Token> Parse(string filePath)
    {
        var tokens = new List<Token>();
        if (File.Exists(filePath))
        {
            Exceptions.FilePath = filePath;
            tokens = new Tokenizer(Exceptions, filePath).Tokens;
        }
        return tokens;
    }

    public Dictionary<int, Dictionary<string, List<TokenInfo>>> AnalyzeTokenInfos(string fileOrFolder)
    {
        var result = new Dictionary<int, Dictionary<string, List<TokenInfo>>>();
        if (File.Exists(fileOrFolder))
        {
            Exceptions.FilePath = fileOrFolder;
            AnalyzeTokenInfos(GetTokenInfos(fileOrFolder), result);
        }
        else if (Directory.Exists(fileOrFolder))
        {
            foreach (var file in Directory.GetFiles(fileOrFolder))
            {
                Exceptions.FilePath = file;
                AnalyzeTokenInfos(GetTokenInfos(file), result);
            }
        }
        return result;
    }

    private List<TokenInfo> GetTokenInfos(string filePath)
    {
        var tokens = new Tokenizer(Exceptions, filePath).Tokens;
        return tokens.Select(token => new TokenInfo(token, filePath)).ToList();
    }

    private static void AnalyzeTokenInfos(List<TokenInfo> infos, Dictionary<int, Dictionary<string, List<TokenInfo>>> map)
    {
        if (infos.Count is 0)
            return;
        var level = infos[0].Token.Level;
        if (!map.ContainsKey(level))
            map[level] = [];
        foreach (var info in infos)
        {
            if (info.Token is Scope scope)
            {
                var property = scope.Property.Select(token => new TokenInfo(token, info.FilePath)).ToList();
                AnalyzeTokenInfos(property, map);
            }
            if (!map[level].ContainsKey(info.Token.Name.Text))
                map[level][info.Token.Name.Text] = [];
            map[level][info.Token.Name.Text].Add(info);
        }
    }
}