using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStartPanel : BasePanel
{
    public Button btnStart;
    public Button btnQuit;
    public RawImage rawImage;
    public Slider slider;
    public InputField inputField;
    public TMP_Dropdown dropdown;
    public Toggle toggle;
    public ScrollRect scrollRect;
    public TMP_InputField tmpInputField;
    public TMP_Text tmpText;



    protected override void Start()
    {
        btnStart.onClick.AddListener(OnBtnStartClick);
        btnQuit.onClick.AddListener(OnBtnQuitClick);
    }

    private void OnBtnStartClick()
    {
        Debug.Log("OnBtnStartClick");
    }

    private void OnBtnQuitClick()
    {
        Debug.Log("OnBtnQuitClick");
    }

    public override void ShowMe()
    {

    }

    public override void HideMe()
    {

    }
}
