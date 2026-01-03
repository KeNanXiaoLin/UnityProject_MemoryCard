using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // 导入DOTween命名空间
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    /// <summary>
    /// 用于存储所有要用到的UI控件，用历史替换原则 父类装子类
    /// </summary>
    protected Dictionary<string, UIBehaviour> controlDic = new Dictionary<string, UIBehaviour>();

    #region 动效相关属性
    /// <summary>
    /// 是否启用显示动效
    /// </summary>
    [Header("动效设置")]
    public bool enableShowAnimation = true;

    /// <summary>
    /// 是否启用隐藏动效
    /// </summary>
    public bool enableHideAnimation = true;

    /// <summary>
    /// 动效持续时间（秒）
    /// </summary>
    public float animationDuration = 0.3f;

    /// <summary>
    /// 缓动函数
    /// </summary>
    public Ease animationEase = Ease.OutQuad;

    /// <summary>
    /// 原始位置（用于恢复）
    /// </summary>
    private Vector3 originalPosition;

    /// <summary>
    /// 原始缩放（用于恢复）
    /// </summary>
    private Vector3 originalScale = Vector3.one;

    /// <summary>
    /// 原始透明度（用于恢复）
    /// </summary>
    private float originalAlpha = 1f;
    #endregion

    protected virtual void Awake()
    {
        if (!enableShowAnimation && !enableHideAnimation)
        {
            return;
        }
        // 保存原始状态
        originalPosition = transform.position;
        originalScale = transform.localScale;

        // 获取CanvasGroup组件，如果没有则添加
        CanvasGroup canvasGroup;
        if (TryGetComponent(out canvasGroup))
        {
            originalAlpha = canvasGroup.alpha;
        }
        else
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            originalAlpha = canvasGroup.alpha;
        }
    }

    protected virtual void Start()
    {

    }

    /// <summary>
    /// 面板显示时会调用的逻辑
    /// </summary>
    public virtual void ShowMe(UnityAction callback = null)
    {
        //执行默认的显示面板想要做的事情
        callback?.Invoke();
    }

    /// <summary>
    /// 面板隐藏时会调用的逻辑
    /// </summary>
    public virtual void HideMe(UnityAction callback = null)
    {
        //执行默认的隐藏面板想要做的事情
        callback?.Invoke();
    }


    #region 动效方法

    /// <summary>
    /// 从下到上弹出显示
    /// </summary>
    /// <param name="offsetY">初始Y轴偏移量</param>
    /// <param name="callback">动画完成回调</param>
    public void ShowFromBottom(float offsetY = 200f, UnityAction callback = null)
    {
        if (!enableShowAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 重置状态
        transform.position = originalPosition + new Vector3(0, -offsetY, 0);
        SetAlpha(0);

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(originalPosition, animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), originalAlpha, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 从小到大缩放显示
    /// </summary>
    /// <param name="scaleFactor">初始缩放因子</param>
    /// <param name="callback">动画完成回调</param>
    public void ShowWithScale(float scaleFactor = 0.5f, UnityAction callback = null)
    {
        if (!enableShowAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 重置状态
        transform.localScale = originalScale * scaleFactor;
        SetAlpha(0);

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(originalScale, animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), originalAlpha, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 淡入显示
    /// </summary>
    /// <param name="callback">动画完成回调</param>
    public void ShowWithFade(UnityAction callback = null)
    {
        if (!enableShowAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 重置状态
        SetAlpha(0);

        // 执行动画
        DOTween.To(() => GetAlpha(), x => SetAlpha(x), originalAlpha, animationDuration)
            .SetEase(animationEase)
            .OnComplete(() => callback?.Invoke());
    }

    /// <summary>
    /// 从左到右滑入显示
    /// </summary>
    /// <param name="offsetX">初始X轴偏移量</param>
    /// <param name="callback">动画完成回调</param>
    public void ShowFromLeft(float offsetX = 200f, UnityAction callback = null)
    {
        if (!enableShowAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 重置状态
        transform.position = originalPosition + new Vector3(-offsetX, 0, 0);
        SetAlpha(0);

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(originalPosition, animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), originalAlpha, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 从上到下滑入显示
    /// </summary>
    /// <param name="offsetY">初始Y轴偏移量</param>
    /// <param name="callback">动画完成回调</param>
    public void ShowFromTop(float offsetY = 200f, UnityAction callback = null)
    {
        if (!enableShowAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 重置状态
        transform.position = originalPosition + new Vector3(0, offsetY, 0);
        SetAlpha(0);

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(originalPosition, animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), originalAlpha, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 从右到左滑入显示
    /// </summary>
    /// <param name="offsetX">初始X轴偏移量</param>
    /// <param name="callback">动画完成回调</param>
    public void ShowFromRight(float offsetX = 200f, UnityAction callback = null)
    {
        if (!enableShowAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 重置状态
        transform.position = originalPosition + new Vector3(offsetX, 0, 0);
        SetAlpha(0);

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(originalPosition, animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), originalAlpha, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 带弹跳效果的显示
    /// </summary>
    /// <param name="callback">动画完成回调</param>
    public void ShowWithBounce(UnityAction callback = null)
    {
        if (!enableShowAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 重置状态
        transform.localScale = Vector3.zero;
        SetAlpha(0);

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(1.2f, animationDuration * 0.6f).SetEase(Ease.OutBack));
        sequence.Append(transform.DOScale(originalScale, animationDuration * 0.4f).SetEase(Ease.InBack));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), originalAlpha, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 向下滑出隐藏
    /// </summary>
    /// <param name="offsetY">Y轴偏移量</param>
    /// <param name="callback">动画完成回调</param>
    public void HideToBottom(float offsetY = 200f, UnityAction callback = null)
    {
        if (!enableHideAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(originalPosition + new Vector3(0, -offsetY, 0), animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), 0, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 缩小隐藏
    /// </summary>
    /// <param name="scaleFactor">最终缩放因子</param>
    /// <param name="callback">动画完成回调</param>
    public void HideWithScale(float scaleFactor = 0.5f, UnityAction callback = null)
    {
        if (!enableHideAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(originalScale * scaleFactor, animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), 0, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 淡出隐藏
    /// </summary>
    /// <param name="callback">动画完成回调</param>
    public void HideWithFade(UnityAction callback = null)
    {
        if (!enableHideAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 执行动画
        DOTween.To(() => GetAlpha(), x => SetAlpha(x), 0, animationDuration)
            .SetEase(animationEase)
            .OnComplete(() => callback?.Invoke());
    }

    /// <summary>
    /// 向左滑出隐藏
    /// </summary>
    /// <param name="offsetX">X轴偏移量</param>
    /// <param name="callback">动画完成回调</param>
    public void HideToLeft(float offsetX = 200f, UnityAction callback = null)
    {
        if (!enableHideAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(originalPosition + new Vector3(-offsetX, 0, 0), animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), 0, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 向上滑出隐藏
    /// </summary>
    /// <param name="offsetY">Y轴偏移量</param>
    /// <param name="callback">动画完成回调</param>
    public void HideToTop(float offsetY = 200f, UnityAction callback = null)
    {
        if (!enableHideAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(originalPosition + new Vector3(0, offsetY, 0), animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), 0, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    /// <summary>
    /// 向右滑出隐藏
    /// </summary>
    /// <param name="offsetX">X轴偏移量</param>
    /// <param name="callback">动画完成回调</param>
    public void HideToRight(float offsetX = 200f, UnityAction callback = null)
    {
        if (!enableHideAnimation)
        {
            callback?.Invoke();
            return;
        }

        // 执行动画
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(originalPosition + new Vector3(offsetX, 0, 0), animationDuration).SetEase(animationEase));
        sequence.Join(DOTween.To(() => GetAlpha(), x => SetAlpha(x), 0, animationDuration).SetEase(animationEase));

        if (callback != null)
        {
            sequence.OnComplete(() => callback());
        }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取当前透明度
    /// </summary>
    /// <returns>透明度值</returns>
    private float GetAlpha()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            return canvasGroup.alpha;
        }
        return 1f;
    }

    /// <summary>
    /// 设置透明度
    /// </summary>
    /// <param name="alpha">透明度值</param>
    private void SetAlpha(float alpha)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    /// <summary>
    /// 重置到初始状态
    /// </summary>
    protected void ResetToInitialState()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        SetAlpha(originalAlpha);
    }

    #endregion
}