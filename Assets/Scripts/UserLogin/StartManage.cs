using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartManage : MonoBehaviour
{
    public static StartManage Instance; // 用于存储 StartManage 脚本的实例（单例模式）
    private TextMeshProUGUI mianMenageText; // 用于存储 TextMeshProUGUI 组件（文本对象）
    private float fadeSpeed = 1f; // 渐变的速度，控制透明度变化的快慢
    private bool isFadingIn = true; // 控制是否正在渐变到不透明（透明度逐渐增大）
    private Coroutine fadingCoroutine; // 用于存储渐变的协程实例

    void Awake()
    {
        Instance = this; // 设置当前对象为 StartManage 的单例
        mianMenageText = GetComponent<TextMeshProUGUI>(); // 获取当前 GameObject 上的 TextMeshProUGUI 组件
        gameObject.SetActive(false); // 禁用 GameObject，开始时不显示
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"跳转到创建玩家界面......");
        }
    }

    // 启动循环的透明到不透明效果
    public void StartFadingLoop()
    {
        if (fadingCoroutine != null)
        {
            StopCoroutine(fadingCoroutine); // 如果之前的渐变协程仍在运行，停止它
        }
        fadingCoroutine = StartCoroutine(FadeTextLoop()); // 启动一个新的渐变协程
    }

    // 实现文本渐变的协程方法（使文本在透明和不透明之间循环变化）
    private IEnumerator FadeTextLoop()
    {
        while (true) // 永久循环，直到手动停止
        {
            Color currentColor = mianMenageText.color; // 获取当前文本的颜色
            float newAlpha = currentColor.a; // 获取当前透明度值（Alpha 值）

            if (isFadingIn) // 如果正在渐变到不透明
            {
                newAlpha += fadeSpeed * Time.deltaTime; // 增加透明度，fadeSpeed 控制变化速度
                if (newAlpha >= 1f) // 如果透明度达到或超过 1（完全不透明）
                {
                    newAlpha = 1f;  // 将透明度设置为 1
                    isFadingIn = false; // 变为渐变到透明
                }
            }
            else // 如果正在渐变到透明
            {
                newAlpha -= fadeSpeed * Time.deltaTime; // 减少透明度，fadeSpeed 控制变化速度
                if (newAlpha <= 0f) // 如果透明度达到或低于 0（完全透明）
                {
                    newAlpha = 0f; // 将透明度设置为 0
                    isFadingIn = true; // 变为渐变到不透明
                }
            }

            // 更新文本的透明度（颜色的透明度部分）
            mianMenageText.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

            // 等待下一帧再继续执行循环
            yield return null;
        }
    }

    public void showPanel()
    {
        gameObject.SetActive(true); // 激活当前面板（显示）
        StartFadingLoop(); // 启动渐变循环
    }

    public void closePanel()
    {
        if (fadingCoroutine != null)
        {
            StopCoroutine(fadingCoroutine); // 停止正在进行的渐变协程
        }

        gameObject.SetActive(false); // 隐藏当前面板
    }
}