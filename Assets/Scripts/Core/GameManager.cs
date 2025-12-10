using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : BaseManager<GameManager>
{
    public LevelData levelData;
    private List<Sprite> sprites;
        
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
        Debug.Log("GameManager Init");
    }

    internal void Init()
    {
        
    }
}
