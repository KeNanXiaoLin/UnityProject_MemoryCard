using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Card : MonoBehaviour
{
    public Renderer frontRenderer;
    public Renderer backRenderer;
    public CardData cardData;
    private bool isFlipping = false;
    private bool isFlipped = false;
    public bool CanFlip => !isFlipping && !isFlipped;
    public bool isFlipStatus => isFlipping || isFlipped;

    // 新增：记录当前卡牌的 Tween，避免重复操作
    private Tween currentTween;

    public void Init(CardData cardData)
    {
        this.cardData = cardData;
        transform.position = cardData.position;
        frontRenderer.material = cardData.material;
        // 初始化时重置旋转和状态
        transform.rotation = Quaternion.identity;
        isFlipping = false;
        isFlipped = false;
    }

    public Tween Flip(Action<Card> onComplete = null)
    {
        if (isFlipping || isFlipped) return null;
        
        // 先 Kill 旧的 Tween（避免残留）
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        isFlipping = true;
        currentTween = transform.DOLocalRotate(new Vector3(0, 180, 0), 0.5f)
            .SetAutoKill(false) // 禁止自动销毁
            .OnComplete(() =>
            {
                isFlipping = false;
                isFlipped = true;
                onComplete?.Invoke(this);
                // 完成后清空 currentTween
                currentTween = null;
            });
        return currentTween;
    }

    public Tween FlipBack(Action<Card> onComplete = null)
    {
        if (isFlipping || !isFlipped) return null;
        
        // 先 Kill 旧的 Tween
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }

        isFlipping = true;
        currentTween = transform.DOLocalRotate(new Vector3(0, 0, 0), 0.5f)
            .SetAutoKill(false)
            .OnComplete(() =>
            {
                isFlipping = false;
                isFlipped = false;
                onComplete?.Invoke(this);
                currentTween = null;
            });
        return currentTween;
    }

    public void SetCardPosition(Vector2 position)
    {
        cardData.position = position;
        transform.position = position;
    }

    public void DestroySelf()
    {
        // 销毁前 Kill 所有关联的 Tween
        if (currentTween != null && currentTween.IsActive())
        {
            currentTween.Kill();
        }
        Destroy(gameObject);
    }

    // 新增：对象销毁时清理 Tween（防护）
    private void OnDestroy()
    {
        if (currentTween != null)
        {
            currentTween.Kill();
            currentTween = null;
        }
    }
}