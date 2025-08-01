using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputPlayerNamePanel : MonoBehaviour
{
    // Instance
    public static InputPlayerNamePanel Instance;
    private TMP_InputField playerNameInput;
    private Button confirmButton;

    private string playerName;
    public string PlayerName
    {
        get { return playerName; }
        set { playerName = value; }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("检测到多个 InputPlayerNamePanel 实例！");
            Destroy(gameObject);  // 防止重复实例化
        }

        // 获取玩家名称输入框
        playerNameInput = transform.Find("InputBG/PlayerNameInputField").GetComponent<TMP_InputField>();
        confirmButton = transform.Find("ConfirmButton").GetComponent<Button>();

        // gameObject.SetActive(false);
    }

    void Start()
    {
        // 输入框监听事件
        playerNameInput.onEndEdit.AddListener(PlayerNameInputOnEndEdit);

        // 按钮监听事件
        confirmButton.onClick.AddListener(OnClickConfirmButton);
    }

    void PlayerNameInputOnEndEdit(string input)
    {
        Debug.Log($"playerNameInput输入了: {input}");
        playerName = input;
        Debug.Log($"playerName: {playerName}");
    }

    private void OnClickConfirmButton()
    {
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log($"playerName 不能为空！");
            return;
        }

        // 打开 选择玩家性别面板
        SelectGenderPanel.Instance.OpenSelectGenderPanel();

        // 关闭当前 输入玩家姓名面板
        CloseInputPlayerNamePanel();
    }

    public void OpenInputPlayerNamePanel()
    {
        gameObject.SetActive(true);
    }

    public void CloseInputPlayerNamePanel()
    {
        gameObject.SetActive(false);
    }
}
