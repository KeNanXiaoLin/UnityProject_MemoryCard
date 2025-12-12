using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData
{
    /// <summary>
    /// 牌的id，用于匹配
    /// </summary>
    public int id;
    /// <summary>
    /// 这里使用的是替换材质的方式实现翻转效果，所以这个字段用不到
    /// </summary>
    public Sprite icon;
    public Material material;
    public Vector2 position;

    public CardData(Material material, int id, Vector2 position)
    {
        this.id = id;
        this.material = material;
        this.position = position;
    }

    public CardData()
    {
        id = 0;
        icon = null;
        material = null;
        position = Vector2.zero;
    }

    public CardData(Material material, int id, float x, float y)
    {
        this.id = id;
        this.material = material;
        this.position = new Vector2(x,y);
    }
}
