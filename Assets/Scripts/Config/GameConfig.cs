using UnityEngine;
using Luban.SimpleJSON;

public static class GameConfig
{
    private static cfg.Tables _tables;

    public static bool IsLoaded => _tables != null;

    public static cfg.Tables Tables => _tables ?? throw new System.InvalidOperationException("配置尚未加载，请先调用 GameConfig.Load()");

    public static void Load()
    {
        if (IsLoaded)
            return;

        cfg.Tables tables = new cfg.Tables(LoadJson);
        _tables = tables;

        Debug.Log("游戏配置加载完毕");
    }

    private static JSONNode LoadJson(string fileName)
    {
        string path = $"Configs/{fileName}";
        TextAsset asset = Resources.Load<TextAsset>(path);

        if (asset == null)
        {
            throw new System.InvalidOperationException($"找不到配置: Assets/Resources/{path}.json");
        }

        return JSON.Parse(asset.text);
    }
}
