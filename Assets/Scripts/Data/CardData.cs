using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData
{
    public Sprite icon;
    public Vector2 position;

    public CardData(Sprite icon, Vector2 position)
    {
        this.icon = icon;
        this.position = position;
    }

    public CardData()
    {
        icon = null;
        position = Vector2.zero;
    }

    public CardData(Sprite icon,float x,float y)
    {
        this.icon = icon;
        this.position = new Vector2(x,y);
    }
}
