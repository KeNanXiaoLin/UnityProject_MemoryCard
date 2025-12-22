using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    private RaycastHit hit;
    private Card firstCard = null;
    private Card secondCard = null;
    private int flipCount = 0;

    // 新增：初始化动画完成标记（关键锁）
    private bool isInitAnimCompleted = false;
    private string prefabLoadPath = "";

    public bool CanFlipNewCard
    {
        get
        {
            // 新增：初始化动画未完成时，禁止翻牌
            if (!isInitAnimCompleted) return false;
            if (flipCount >= 2) return false;
            return true;
        }
    }

    void Awake()
    {
        prefabLoadPath = ResLoadSettings.Instance.GetPrefabPath;
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        // 重置初始化动画标记
        isInitAnimCompleted = false;
        // 清空旧卡牌（避免残留）
        foreach (var card in cards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        cards.Clear();

        // 加载卡牌逻辑不变
        for (int i = 0; i < GameManager.Instance.cardDatas.Count; i++)
        {
            ResLoadMgr.Instance.LoadRes<GameObject>(prefabLoadPath,"Card",(res)=>
            {
                Card card = GameObject.Instantiate(res).GetComponent<Card>();
                card.Init(GameManager.Instance.cardDatas[i]);
                cards.Add(card);
            },true);
        }
        Debug.Log("Current Card Count: " + cards.Count);
        Shuffle();
        // 播放初始化动画，并等待完成后解锁点击
        StartCoroutine(InitAndUnlockClickCoroutine());
    }

    // 新增：初始化动画完成后解锁点击
    private IEnumerator InitAndUnlockClickCoroutine()
    {
        yield return StartCoroutine(ShowAllCardCoroutine(1));
        // 初始化动画全部完成，解锁点击
        isInitAnimCompleted = true;
    }

    void Update()
    {
        // 移除：Update中重复调用CheckMatch（原逻辑会导致多次触发，改为仅在第二张牌翻完后调用）
        ClickCard();
    }

    private void ClickCard()
    {
        if (!CanFlipNewCard) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << LayerMask.NameToLayer("Card")))
            {
                Card card = hit.transform.GetComponent<Card>();
                if (card != null && card.CanFlip)
                {
                    flipCount++;
                    card.Flip((c) =>
                    {
                        if (firstCard == null)
                        {
                            firstCard = c;
                        }
                        else
                        {
                            secondCard = c;
                            CheckMatch(); // 仅在第二张牌翻完后调用一次
                        }
                    });
                }
            }
        }
    }

    public void CheckMatch()
    {
        if (firstCard == null || secondCard == null) return;

        // 匹配成功逻辑
        if (firstCard.cardData.id == secondCard.cardData.id)
        {
            var delOne = firstCard;
            var delTwo = secondCard;

            // Tween 加 SetAutoKill(false) 避免自动销毁
            Tween tweenOne = delOne.transform.DOLocalMove(delOne.transform.position + Vector3.up * 20, 0.5f)
                .SetAutoKill(false)
                .OnComplete(() =>
                {
                    delOne.DestroySelf();
                });
            Tween tweenTwo = delTwo.transform.DOLocalMove(delTwo.transform.position + Vector3.up * 20, 0.5f)
                .SetAutoKill(false)
                .OnComplete(() =>
                {
                    delTwo.DestroySelf();
                });

            cards.Remove(firstCard);
            cards.Remove(secondCard);
            firstCard = null;
            secondCard = null;

            StartCoroutine(CheckAllAnimCompleteCoroutine(tweenOne, tweenTwo));
        }
        else
        {
            // 匹配失败逻辑：Tween 加 SetAutoKill(false)
            Tween tweenFirst = firstCard.FlipBack((c) =>
            {
                firstCard = null;
            }).SetAutoKill(false);
            Tween tweenSecond = secondCard.FlipBack((c) =>
            {
                secondCard = null;
            }).SetAutoKill(false);

            StartCoroutine(CheckAllAnimCompleteCoroutine(tweenFirst, tweenSecond));
        }
    }

    public void CheckWin()
    {
        if (cards.Count == 0)
        {
            // 通关前先锁定点击，避免动画过程中重复触发
            isInitAnimCompleted = false;
            GameManager.Instance.GoToNextLevel();
            Init();
        }
    }

    public void Shuffle()
    {
        List<Vector2> positions = new List<Vector2>();
        foreach (Card card in cards)
        {
            positions.Add(card.cardData.position);
        }
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Vector2 temp = positions[i];
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetCardPosition(positions[i]);
        }
    }

    // 原 ShowAllCard 改为仅启动协程，不再直接调用
    public void ShowAllCard(float time)
    {
        StartCoroutine(ShowAllCardCoroutine(time));
    }

    private IEnumerator ShowAllCardCoroutine(float time)
    {
        List<Tween> initTweens = new List<Tween>();
        // 翻牌到正面：记录所有 Tween，加 SetAutoKill(false)
        foreach (Card card in cards)
        {
            Tween t = card.Flip().SetAutoKill(false);
            if (t != null) initTweens.Add(t);
        }
        // 等待所有翻牌动画完成
        foreach (var t in initTweens)
        {
            if (t != null) yield return new WaitUntil(() => !t.IsPlaying());
        }

        yield return new WaitForSeconds(time);

        // 翻牌回背面：记录所有 Tween
        initTweens.Clear();
        foreach (Card card in cards)
        {
            Tween t = card.FlipBack().SetAutoKill(false);
            if (t != null) initTweens.Add(t);
        }
        // 等待所有翻回动画完成
        foreach (var t in initTweens)
        {
            if (t != null) yield return new WaitUntil(() => !t.IsPlaying());
        }
    }

    // 改造：Tween 安全检查，避免引用已销毁的 Tween
    private IEnumerator CheckAllAnimCompleteCoroutine(params Tween[] tweens)
    {
        foreach (Tween tween in tweens)
        {
            // 关键：检查 Tween 是否有效（未被 Kill/销毁）
            if (tween == null || !tween.IsActive()) continue;
            yield return new WaitUntil(() => !tween.IsPlaying());
            // 手动 Kill 并回收 Tween（避免内存泄漏）
            tween.Kill();
        }
        flipCount = 0;
        CheckWin();
    }
}