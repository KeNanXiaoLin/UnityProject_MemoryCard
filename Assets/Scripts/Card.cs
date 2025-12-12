using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private CardData cardData;

    public void Init(CardData cardData)
    {
        this.cardData = cardData;
        transform.position = cardData.position;
    }


}
