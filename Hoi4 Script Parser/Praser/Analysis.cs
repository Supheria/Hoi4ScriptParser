using Parser.data;

namespace Parser;

public class Analysis
{
    public class TokenInfo
    {
        private Token Token { get; }
        private string FilePath { get; }

        public TokenInfo(Token token, string filePath)
        {
            Token = token;
            FilePath = filePath;
        }
    }

    public Dictionary<uint, Dictionary<string, List<Token>>> LevelMap { get; } = new();

    public Analysis()
    {
        // 防止为空导致索引不存在
        LevelMap[0] = new() { [""] = new() };
    }

    public Analysis(string fileOrFolder, string logDir)
    {
        Exceptions.LogPath = Path.Combine(logDir, "ex.log");
        if (File.Exists(fileOrFolder))
        {
            CacheMap(GetTokenList(fileOrFolder), LevelMap);
        }
        else if (Directory.Exists(fileOrFolder))
        {
            foreach (var file in Directory.GetFiles(fileOrFolder))
            {
                CacheMap(GetTokenList(file), LevelMap);
            }
        }
    }

    private static List<Token> GetTokenList(string filePath)
    {
        List<Token> tokens = new();
        _ = new Tokenizer(filePath, tokens);
        return tokens;
    }

    private static void CacheMap(List<Token> tokens, Dictionary<uint, Dictionary<string, List<Token>>> map)
    {
        if (tokens.FirstOrDefault() == null)
            return;
        var level = tokens.First().Level;
        if (!map.ContainsKey(level))
        {
            map[level] = new();
        }
        foreach (var token in tokens)
        {
            if (token is Scope scope)
            {
                CacheMap(scope.Property, map);
            }
            if (!map[level].ContainsKey(token.Name))
            {
                map[level][token.Name] = new();
            }
            map[level][token.Name].Add(token);
        }
    }
}