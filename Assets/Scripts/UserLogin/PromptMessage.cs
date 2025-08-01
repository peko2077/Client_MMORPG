using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptMessage : MonoBehaviour
{
    public static PromptMessage Instance;

    private TextMeshProUGUI messageText;

    // 标记是否显示信息
    private bool isShowing = false;

    void Awake()
    {
        Instance = this;
        messageText = transform.Find("MessageBG/MessageText").GetComponent<TextMeshProUGUI>();

        // 确保 PromptMessage 物体在启动时启用
        gameObject.SetActive(false); // 一开始隐藏，但实例化
    }

    void Start()
    {

    }

    void Update()
    {
        // 如果正在显示提示信息，监听用户点击任意地方
        if (isShowing && Input.GetMouseButtonDown(0))  // 0 是鼠标左键
        {
            CloseInfo();
        }
    }

    // 打开 提示窗口
    public void ShowInfo(string info)
    {
        gameObject.SetActive(true); // 显示信息框
        messageText.text = info;    // 设置信息
        isShowing = true;           // 标记正在显示
    }

    // 关闭 提示窗口
    public void CloseInfo()
    {
        gameObject.SetActive(false); // 隐藏信息框
        isShowing = false;            // 标记不再显示
    }

}
