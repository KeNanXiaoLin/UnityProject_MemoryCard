using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : BaseManager<GameManager>
{
    public LevelData levelData;
    // 现在的实现方式用不到这个字段
    // private List<Sprite> sprites;
    private List<Material> materials;
    /// <summary>
    /// 当前的关卡
    /// </summary>
    private int curLevel = 1;
    /// <summary>
    /// 当前关卡的牌组
    /// </summary>
    public List<CardData> cardDatas = new List<CardData>();
        
    private GameManager()
    {
        levelData = new LevelData();
        
        Material[] materials = ResMgr.Instance.LoadAll<Material>("Materials");
        this.materials = new List<Material>(materials);
        
    }

    public void Init()
    {
        //构建当前关卡的数据
        //构建当前关卡的牌组,2个一组
        SingleLevelData levelData = this.levelData.GetLevelData(curLevel);
        int index = 0;
        for (int i = 0; i < levelData.rowCount; i++)
        {
            for (int j = 0; j < levelData.colCount; j++)
            {
                index = i*levelData.colCount + j;
                int id = index / 2;
                Material material = materials[index / 2];
                float x = levelData.startX + j*levelData.cardWidth + j*levelData.spacingX;
                float y = levelData.startY - i*levelData.cardHeight - i*levelData.spacingY;
                cardDatas.Add(new CardData(material,id,x,y));
            }
        }
        Debug.Log("GameManager Init");
    }

    internal void GoToNextLevel()
    {
        cardDatas.Clear();
        curLevel++;
        Init();
    }
}
