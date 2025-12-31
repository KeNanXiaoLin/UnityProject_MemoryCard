using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    // public SpriteRenderer spriteRenderer;
    private Button btnTest;
    private Toggle toggleTest;
    private Slider slider;
    private Image imgTest;
    private RawImage rawImageTest;
    private Text textTest;
    private TextMeshProUGUI textMeshProUGUI;
    private TextMeshPro textMeshPro;
    private Dropdown dropdownTest;
    // Start is called before the first frame update
    void Start()
    {
        // btnTest = GetComponent<Button>();
        // toggleTest = GetComponent<Toggle>();
        slider = GetComponent<Slider>();
        // imgTest = GetComponent<Image>();
        // rawImageTest = GetComponent<RawImage>();
        // textTest = GetComponent<Text>();
        // textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        // textMeshPro = GetComponent<TextMeshPro>();
        // dropdownTest = GetComponent<Dropdown>();
        // Sprite sprite = Resources.Load<Sprite>("Sprites/style_1_01");
        // spriteRenderer.sprite = sprite;
        // btnTest.onClick.AddListener(clickTest);
        // toggleTest.onValueChanged.AddListener(toggleTestValueChanged);
        slider.onValueChanged.AddListener((value) =>
        {
            Debug.Log("slider: " + value);
        });
    }

    public void clickTest()
    {
        Debug.Log("clickTest");
    }

    private void toggleTestValueChanged(bool value)
    {
        Debug.Log("toggleTestValueChanged: " + value);
    }
    
}
