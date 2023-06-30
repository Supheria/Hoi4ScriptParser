using Parser.Data.TokenTypes;

namespace Parser.Helper;

public class Analysis
{
    //public Analysis()
    //{
    //    // 防止为空导致索引不存在
    //    LevelMap[0] = new() { [""] = new() };
    //}

    private readonly Exceptions _exceptions = new();
    private string _errorLog = "";

    public static void Parse(string fileOrFolder, out Dictionary<uint, Dictionary<string, List<TokenInfo>>> result, out string errorLog)
    {
        var analysis = new Analysis();
        analysis.Parse(fileOrFolder, out result);
        errorLog = analysis._errorLog;
    }

    private void Parse(string fileOrFolder, out Dictionary<uint, Dictionary<string, List<TokenInfo>>> result)
    {
        result = new();

        if (File.Exists(fileOrFolder))
        {
            _exceptions.FilePath = fileOrFolder;
            CacheMap(GetTokenInfoList(fileOrFolder), result);
        }
        else if (Directory.Exists(fileOrFolder))
        {
            foreach (var file in Directory.GetFiles(fileOrFolder))
            {
                _exceptions.FilePath = file;
                CacheMap(GetTokenInfoList(file), result);
            }
        }

        _errorLog = _exceptions.ErrorString.ToString();
    }

    private IEnumerable<TokenInfo> GetTokenInfoList(string filePath)
    {
        var tokens = Tokenizer.Tokenize(filePath, _exceptions);
        return tokens.Select(token => new TokenInfo(token, filePath));
    }

    private void CacheMap(IEnumerable<TokenInfo> infos, Dictionary<uint, Dictionary<string, List<TokenInfo>>> map)
    {
        var infoArray = infos.ToArray();
        if (infoArray.Length is 0)
            return;
        var level = infoArray[0].Token.Level;
        if (!map.ContainsKey(level))
        {
            map[level] = new();
        }
        foreach (var info in infoArray)
        {
            if (info.Token is Scope scope)
            {
                var property = scope.Property.Select(token => new TokenInfo(token, info.FilePath)).ToList();
                CacheMap(property, map);
            }
            if (!map[level].ContainsKey(info.Token.Name.Text))
            {
                map[level][info.Token.Name.Text] = new();
            }
            map[level][info.Token.Name.Text].Add(info);
        }
    }
}