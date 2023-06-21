using Parser.data;

namespace Parser.helper;

public class Analysis
{
    public Dictionary<uint, Dictionary<string, List<TokenInfo>>> LevelMap { get; } = new();

    public string ExceptionLogPath => Exceptions.LogPath;

    public Analysis()
    {
        // 防止为空导致索引不存在
        LevelMap[0] = new() { [""] = new() };
    }

    public Analysis(string fileOrFolder, string logDir)
    {
        Exceptions.LogPath = Path.Combine(logDir, "hoi4 script parse exception.txt");
        if (File.Exists(fileOrFolder))
        {
            Exceptions.FilePath = fileOrFolder;
            CacheMap(GetTokenInfoList(fileOrFolder), LevelMap);
        }
        else if (Directory.Exists(fileOrFolder))
        {
            foreach (var file in Directory.GetFiles(fileOrFolder))
            {
                Exceptions.FilePath = file;
                CacheMap(GetTokenInfoList(file), LevelMap);
            }
        }
    }

    private static List<TokenInfo> GetTokenInfoList(string filePath)
    {
        List<Token> tokens = new();
        _ = new Tokenizer(filePath, tokens);
        return tokens.Select(token => new TokenInfo(token, filePath)).ToList();
    }

    private static void CacheMap(List<TokenInfo> infoList, Dictionary<uint, Dictionary<string, List<TokenInfo>>> map)
    {
        if (infoList.FirstOrDefault() == null)
            return;
        var level = infoList.First().Level;
        if (!map.ContainsKey(level))
        {
            map[level] = new();
        }
        foreach (var info in infoList)
        {
            if (info.Token is Scope scope)
            {
                var property = scope.Property.Select(token => new TokenInfo(token, info.FilePath)).ToList();
                CacheMap(property, map);
            }
            if (!map[level].ContainsKey(info.Name))
            {
                map[level][info.Name] = new();
            }
            map[level][info.Name].Add(info);
        }
    }
}