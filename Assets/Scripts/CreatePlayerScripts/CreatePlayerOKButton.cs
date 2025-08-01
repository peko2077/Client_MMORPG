using System.Collections;
using System.Collections.Generic;
using Mymmorpg;
using UnityEngine;
using UnityEngine.UI;

public class CreatePlayerOKButton : MonoBehaviour
{
    public static CreatePlayerOKButton Instance;

    private Button createPlayerOKButton;

    private int userId = 2; // 假设用户已登录，userId 可在登录成功后传入
    private string playerName;
    private bool gender;

    private int step = 0;

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

        createPlayerOKButton = GetComponent<Button>();

        gameObject.SetActive(false);
    }

    void Start()
    {
        createPlayerOKButton.onClick.AddListener(OnclickCreatePlayerOKButton);

        NetworkClient.Instance.OnApiResponse += OnApiResponse; // 订阅响应事件
    }

    void OnDestroy()
    {
        // 解绑事件，避免重复触发或内存泄漏
        if (NetworkClient.Instance != null)
            NetworkClient.Instance.OnApiResponse -= OnApiResponse;
    }

    private void OnclickCreatePlayerOKButton()
    {
        createPlayerOKButton.interactable = false;

        playerName = InputPlayerNamePanel.Instance.PlayerName;
        Debug.Log($"[CreatePlayerManage] playerName = {playerName}");

        gender = SelectGenderPanel.Instance.Gender;
        Debug.Log($"[CreatePlayerManage] gender = {gender} = {(gender ? "女" : "男")}");

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log($"[CreatePlayerOKButton]: 点击失败, playerName不能为空");
            return;
        }

        // 第一步：发送 AddPlayer 请求
        ApiRequest request = new ApiRequest
        {
            Command = "AddPlayer",
            AddPlayer = new AddPlayerRequest
            {
                PlayerName = playerName,
                Gender = gender,
            }
        };

        step = 1; // 标记当前是创建角色阶段

        NetworkClient.Instance.SendRequest(request); // 发送请求
    }

    private void OnApiResponse(ApiResponse response)
    {
        Debug.Log("收到服务器响应: " + response.Message);
        // 根据 response 做进一步处理

        // 步骤1：收到添加角色的响应
        if (step == 1 && response.Success && response.Player != null)
        {
            Debug.Log($"角色创建成功: {response.Player.PlayerName}, ID: {response.Player.PlayerId}");

            // 第二步：更新该角色的 userId
            ApiRequest updateRequest = new ApiRequest
            {
                Command = "UpdatePlayer",
                UpdatePlayer = new UpdatePlayerRequest
                {
                    PlayerId = response.Player.PlayerId,
                    UserId = userId
                }
            };

            step = 2;
            NetworkClient.Instance.SendRequest(updateRequest);
        }
        // 步骤2：收到更新 userId 的响应
        else if (step == 2)
        {
            if (response.Success)
            {
                Debug.Log($"用户绑定成功: {response.Message}");

                // 关闭此页面
                CloseButton();
            }
            else
            {
                Debug.LogWarning($"绑定失败: {response.Error}");

                // 返回从新创建玩家
            }

            // 完成后恢复按钮
            createPlayerOKButton.interactable = true;
            step = 0;
        }

    }

    public void OpenButton()
    {
        gameObject.SetActive(true);
    }

    public void CloseButton()
    {
        gameObject.SetActive(false);
    }
}