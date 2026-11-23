using System;
using UnityEngine;

public struct CraftResult
{
    public int ItemId;
    public int CraftCount;
}

public struct ColRowResult
{
    public int MinCol;
    public int MaxCol;
    public int MinRow;
    public int MaxRow;

    public int Width;
    public int Height;

    public System.Collections.Generic.List<int> Sequence;
}

public static class Crafter
{
    /// <summary>
    /// key: 宽|高|sequence
    /// </summary>
    private static readonly System.Collections.Generic.Dictionary<
        string,
        cfg.CraftingRecipeConfig
    > craftingRecipeMap = new();

    static Crafter()
    {
        foreach (cfg.CraftingRecipeConfig config in GameConfig.Tables.TbCraftingRecipe.DataList)
        {
            string keyString =
                $"{config.Width}|{config.Height}|{string.Join(",", config.Ingredients)}";
            if (!craftingRecipeMap.TryAdd(keyString, config))
                throw new System.InvalidOperationException($"配方重复: {keyString}");
        }
    }

    private static int RowCount = 3;
    private static int ColCount = 3;

    public static CraftResult CraftSomething(int[] itemSequence)
    {
        int[] mirrorItemSequence = new int[itemSequence.Length];
        Array.Copy(itemSequence, 0, mirrorItemSequence, 0, mirrorItemSequence.Length);
        for (int row = 0; row < RowCount; row++)
        {
            int rowStartIdx = row * RowCount;
            int rowEndIdx = (row + 1) * RowCount - 1;

            int tmp = mirrorItemSequence[rowStartIdx];
            mirrorItemSequence[rowStartIdx] = mirrorItemSequence[rowEndIdx];
            mirrorItemSequence[rowEndIdx] = tmp;
        }

        ColRowResult comparisonResult = ComparisonSequence(itemSequence);
        ColRowResult comparisonMirrorResult = ComparisonSequence(mirrorItemSequence);

        CraftResult result = new CraftResult();
        string keyString =
            $"{comparisonResult.Width}|{comparisonResult.Height}|{string.Join(",", comparisonResult.Sequence)}";
        string keyMirrorString =
            $"{comparisonMirrorResult.Width}|{comparisonMirrorResult.Height}|{string.Join(",", comparisonMirrorResult.Sequence)}";
        bool suc =
            craftingRecipeMap.TryGetValue(keyString, out cfg.CraftingRecipeConfig config)
            || craftingRecipeMap.TryGetValue(keyMirrorString, out config);

        if (!suc)
        {
            result.ItemId = -1;
            result.CraftCount = -1;
        }
        else
        {
            result.ItemId = config.ResultItemId;
            result.CraftCount = config.ResultCount;
        }

        return result;
    }

    private static ColRowResult ComparisonSequence(int[] sequence)
    {
        int width = 0;
        int height = 0;

        int minCol = ColCount;
        int maxCol = -1;
        int minRow = RowCount;
        int maxRow = -1;

        ColRowResult result = new ColRowResult();
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                int itemIdx = (row * ColCount) + col;
                int itemId = sequence[itemIdx];
                if (itemId <= 0)
                    continue;

                minCol = Mathf.Min(minCol, col);
                maxCol = Mathf.Max(maxCol, col);

                minRow = Mathf.Min(minRow, row);
                maxRow = Mathf.Max(maxRow, row);
            }
        }

        if (maxRow == -1) { } //itemSequence为空
        else
        {
            width = maxCol - minCol + 1;
            height = maxRow - minRow + 1;
        }

        //minRow, maxRow, minCol, maxCol 围成最小物品矩形
        System.Collections.Generic.List<int> itemSequenceHandle = new();
        for (int row = minRow; row <= maxRow; row++)
        {
            for (int col = minCol; col <= maxCol; col++)
            {
                int itemIdx = (row * ColCount) + col;
                itemSequenceHandle.Add(sequence[itemIdx]);
            }
        }

        result = new ColRowResult()
        {
            Width = width,
            Height = height,
            Sequence = itemSequenceHandle,

            MinCol = minCol,
            MaxCol = maxCol,
            MinRow = minRow,
            MaxRow = maxRow,
        };

        return result;
    }
}
