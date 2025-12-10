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
}

[System.Serializable]
public class SingleLevelData
{
    public int id;
    public int level;
    public float startX;
    public float startY;
    public float spacingX;
    public float spacingY;
    public int rowCount;
    public int colCount;
}
