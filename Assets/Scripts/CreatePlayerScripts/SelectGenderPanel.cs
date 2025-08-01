using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectGenderPanel : MonoBehaviour, IPointerClickHandler
{
    public static SelectGenderPanel Instance;
    private Button confirmButton;

    bool isSelected = false;

    private bool gender;
    public bool Gender
    {
        get { return gender; }
        set { gender = value; }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("检测到多个 CreatePlayerOKButton 实例！");
            Destroy(gameObject);  // 防止重复实例化
        }

        confirmButton = transform.Find("ConfirmButton").GetComponent<Button>();

        gameObject.SetActive(false);
    }

    void Start()
    {
        confirmButton.onClick.AddListener(OnClickConfirmButton);

        confirmButton.interactable = false; // 确认按钮初始为不可用
    }

    void Update()
    {

    }

    private void OnClickConfirmButton()
    {
        if (!isSelected)
        {
            Debug.Log($"请选择玩家性别......");
            return;
        }

        Debug.Log($"玩家选择了 {(gender ? "女" : "男")}");

        // 打开提交按钮
        CreatePlayerOKButton.Instance.OpenButton();

        // 关闭当前 玩家选择性别面板
        CloseSelectGenderPanel();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[SelectGenderPanel] 点击了: {eventData.pointerCurrentRaycast.gameObject.name}");
        if (eventData.pointerCurrentRaycast.gameObject.name == "MaleImage")
        {
            // Debug.Log($"[SelectGenderPanel] 点击了: {eventData.pointerCurrentRaycast.gameObject.name}");
            if (!isSelected || gender != false)
            {
                gender = false;
                isSelected = true;
                UpdateConfirmButtonState(); // 更新确认按钮状态
            }
        }

        if (eventData.pointerCurrentRaycast.gameObject.name == "FemaleImage")
        {
            // Debug.Log($"[SelectGenderPanel] 点击了: {eventData.pointerCurrentRaycast.gameObject.name}");
            if (!isSelected || gender != true)
            {
                gender = true;
                isSelected = true;
                UpdateConfirmButtonState(); // 更新确认按钮状态
            }
        }

        Debug.Log($"[SelectGenderPanel] 选择的性别: {(gender ? "女" : "男")}");
    }

    // 更新确认按钮状态
    private void UpdateConfirmButtonState()
    {
        confirmButton.interactable = true; // 激活确认按钮
    }

    public void OpenSelectGenderPanel()
    {
        gameObject.SetActive(true);
    }

    public void CloseSelectGenderPanel()
    {
        gameObject.SetActive(false);
    }
}