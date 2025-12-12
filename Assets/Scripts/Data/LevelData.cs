using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    public List<SingleLevelData> levels;

    public LevelData()
    {
        string path = Application.streamingAssetsPath + "/LevelData.json";
        levels = JsonMgr.Instance.LoadDataFromFilePath<List<SingleLevelData>>(path);
        if (levels == null)
        {
            Debug.LogError("LevelData is null,please check the file path: " + path);
        }
    }

    public SingleLevelData GetLevelData(int level)
    {
        foreach (var item in levels)
        {
            if (item.level == level)
            {
                return item;
            }
        }
        Debug.LogError("LevelData is null,please check the level: " + level);
        return null;
    }
}

[System.Serializable]
public class SingleLevelData
{
    /// <summary>
    /// 关卡id
    /// </summary>
    public int id;
    /// <summary>
    /// 关卡等级
    /// </summary>
    public int level;
    /// <summary>
    /// 卡牌的起始x坐标
    /// </summary>
    public float startX;
    /// <summary>
    /// 卡牌的起始y坐标
    /// </summary>
    public float startY;
    /// <summary>
    /// 卡牌的x轴间距
    /// </summary>
    public float spacingX;
    /// <summary>
    /// 卡牌的y轴间距
    /// </summary>
    public float spacingY;
    /// <summary>
    /// 卡牌的行数
    /// </summary>
    public int rowCount;
    /// <summary>
    /// 卡牌的列数
    /// </summary>
    public int colCount;
    /// <summary>
    /// 卡牌的牌数
    /// </summary>
    public int cardCount;
    public float cardWidth;
    /// <summary>
    /// 卡牌的牌高
    /// </summary>
    public float cardHeight;
}
