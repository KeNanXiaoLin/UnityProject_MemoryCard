using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : BaseManager<GameManager>
{
    public LevelData levelData;
    private List<Sprite> sprites;
    /// <summary>
    /// 当前的关卡
    /// </summary>
    private int curLevel = 1;
    /// <summary>
    /// 当前关卡的牌组
    /// </summary>
    private List<CardData> cardDatas = new List<CardData>();
        
    private GameManager()
    {
        levelData = new LevelData();
        
        sprites = new List<Sprite>();
        string suffix = "";
        for (int i = 0; i < 25; i++)
        {
            suffix = (i + 1) < 10 ? "0" + (i + 1) : (i + 1).ToString();
            sprites.Add(ResMgr.Instance.Load<Sprite>("Sprites/style_1_" + suffix));
        }
        
    }

    public void Init()
    {
        //构建当前关卡的牌组,2个一组
        SingleLevelData levelData = this.levelData.GetLevelData(curLevel);
        int index = 0;
        for (int i = 0; i < levelData.rowCount; i++)
        {
            for (int j = 0; j < levelData.colCount; j++)
            {
                index = i*levelData.colCount + j;
                Sprite icon = sprites[index / 2];
                float x = levelData.startX + j*levelData.cardWidth + j*levelData.spacingX;
                float y = levelData.startY - i*levelData.cardHeight - i*levelData.spacingY;
                cardDatas.Add(new CardData(icon,x,y));
            }
        }
        //在场景中实例化牌组
        for (int i = 0; i < cardDatas.Count; i++)
        {
            GameObject cardPrefab = ResMgr.Instance.Load<GameObject>("Prefabs/Card");
            Card card = GameObject.Instantiate(cardPrefab).GetComponent<Card>();
            card.Init(cardDatas[i]);
        }
        Debug.Log("GameManager Init");
    }
}
