using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginLoading : MonoBehaviour
{
    private static LoginLoading Instance;
    private Slider loadingSlider;
    private TextMeshProUGUI sliderValueText;
    private TextMeshProUGUI loadingText;
    bool isLoading = false;

    void Awake()
    {
        Instance = this;
        // closePanel();
    }
    void Start()
    {
        loadingSlider = transform.Find("LoadingSlider").GetComponent<Slider>();
        loadingSlider.interactable = false;
        loadingSlider.value = 0f;
        Debug.Log($"loadingSlider = {loadingSlider.value}");

        sliderValueText = transform.Find("SliderValueText").GetComponent<TextMeshProUGUI>();
        sliderValueText.text = "0.00%";
        Debug.Log($"loadingSlider = {sliderValueText.text}");

        loadingText = transform.Find("LoadingText").GetComponent<TextMeshProUGUI>();

        loadingSlider.onValueChanged.AddListener(SliderOnValueChanged);

        // 只在第一次调用时启动 InvokeRepeating
        if (!isLoading)
        {
            InvokeRepeating("ChangeLoadingSliderValue", 0, 1);
            isLoading = true;
        }
    }

    void Update()
    {
        if (loadingSlider.value >= 0.99f && isLoading)
        {
            CancelInvoke("ChangeLoadingSliderValue");
            loadingSlider.value = 1f; // 确保进度条值接近 1 但不超过 1
            isLoading = false;
            Debug.Log($"停止循环ChangeLoadingSliderValue");
        }

        // 测试模拟资源加载完成
        if (Input.GetMouseButtonDown(0))
        {
            loadingSlider.value = 1;
        }

        if (loadingSlider.value == 1)
        {
            loadingText.text = "加载完成！";
            MianManege.Instance.showPanel();
            closePanel();
            // Invoke("OpenManege", 1.5f);
        }
    }

    public void showPanel()
    {
        gameObject.SetActive(true);
    }

    public void closePanel()
    {
        // 确保取消 InvokeRepeating
        CancelInvoke("ChangeLoadingSliderValue");
        gameObject.SetActive(false);
    }

    string ConvertToPercentage(float volumeValue)               //  将传入的Float值 转化为整数Int 再变成百分比
    {
        // 格式化为两位小数
        return (volumeValue * 100).ToString("F2") + "%";        // 保留两位小数
    }

    void SliderOnValueChanged(float value)
    {
        sliderValueText.text = ConvertToPercentage(value);
    }

    void ChangeLoadingSliderValue()
    {
        // 生成 0.05 到 0.1 之间的随机数
        float randomValue = Random.Range(0.08f, 0.1f);

        // 当进度条值接近 1 时，逐渐减小每次增加的值
        if (loadingSlider.value >= 0.95f)
        {
            randomValue = Random.Range(0.01f, 0.03f); // 在接近 1 时，减少增量
        }

        // 确保进度条不会超出最大值 1
        loadingSlider.value = Mathf.Min(loadingSlider.value + randomValue, 1f);

        // Debug.Log($"loadingSlider.value = {loadingSlider.value.ToString("F2")}");
    }

    void OpenManege()
    {
        MianManege.Instance.showPanel();
        closePanel();
    }
}
