using System.Collections.Generic;
using Mymmorpg;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Client.service;

public class UserLogin : MonoBehaviour
{
    public static UserLogin Instance;

    // 用于延迟验证的标志
    private bool isUserNameValid = false;  // 用户名是否有效
    private bool isPasswordValid = false;  // 密码是否有效

    // 定义 TMP_InputField 类型的变量，分别用于用户名和密码输入框
    private TMP_InputField userNameInput;
    private TMP_InputField passwordInput;

    // 定义一个 Button 变量，用于确认按钮
    private Button OKButton;
    private Button backButton;

    private int step = 0;

    private int userId;

    private List<Player> players = new();
    public List<Player> Players => players; // 公有只读属性（其他类只能读，不能直接修改）

    void Awake()
    {
        Instance = this;

        // 获取输入框组件（用户输入框）
        userNameInput = transform.Find("UserNameInput").GetComponent<TMP_InputField>();
        userNameInput.onEndEdit.AddListener(OnUserNameEditEnd);

        // 获取输入框组件（密码输入框）
        passwordInput = transform.Find("PasswordInput").GetComponent<TMP_InputField>();
        passwordInput.onEndEdit.AddListener(OnPasswordEditEnd);

        // 获取按钮组件
        OKButton = transform.Find("ConfirmButton").GetComponent<Button>();
        backButton = transform.Find("BackButton").GetComponent<Button>();

        // 提交按钮默认关闭
        OKButton.interactable = false;

        gameObject.SetActive(false); // 关闭该窗口
    }

    void Start()
    {
        // 初始化表单验证
        ValidateForm();

        // 监听按钮事件
        OKButton.onClick.AddListener(OnClickOKButton);
        backButton.onClick.AddListener(OnClickBackButton);

        // 订阅响应事件
        NetworkClient.Instance.OnApiResponse += OnApiResponse;
    }

    void OnDestroy()
    {
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.OnApiResponse -= OnApiResponse;
        }
    }

    void OnUserNameEditEnd(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            ErrorPrompt.Instance.ShowErrorPrompt("用户名不能为空");
            isUserNameValid = false;
        }
        else
        {
            isUserNameValid = true;
        }

        // 调用表单验证，确保按钮状态正确
        ValidateForm();
    }

    void OnPasswordEditEnd(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            ErrorPrompt.Instance.ShowErrorPrompt("密码不能为空");
            isPasswordValid = false;
        }
        else
        {
            isPasswordValid = true;
        }

        // 调用表单验证，确保按钮状态正确
        ValidateForm();
    }

    void OnClickBackButton()
    {
        MianManege.Instance.showPanel();
        closePanel();
    }

    void OnClickOKButton()
    {
        // 禁用按钮以防止重复点击
        OKButton.interactable = false;

        // 获取用户输入的用户名和密码
        string userName = userNameInput.text;
        string password = passwordInput.text;

        // 使用封装后的 Service 来发送登录请求
        LoginUserService.LoginUser(userName, password);

        step = 1; // 标记当前是 用户登录 阶段
    }

    private void OnApiResponse(ApiResponse response)
    {
        Debug.Log("[UserLogin] 收到服务器响应: " + response.Message);

        if (response.Command == "LoginUser")
        {
            HandleloginResponse(response);
        }
        else if (response.Command == "GetPlayersByUserId")
        {
            HandlePlayerListResponse(response);
        }
        else
        {
            Debug.Log($"未处理的命令: {response.Command}");
        }

    }

    private void HandleloginResponse(ApiResponse response)
    {
        if (response.Success)
        {
            User user = response.Session.Data.User;

            Debug.Log($"[UserLogin.cs] [OnApiResponse] 用户登录成功, ID: {user.UserId}, userName: {user.UserName}");

            // 将登录之后获取到的userId提取出来
            userId = user.UserId;

            step = 2;  // 成功后进入下一阶段

            // 显示提示信息面板
            PromptMessage.Instance.ShowInfo(response.Message);

            Debug.Log($"准备请求玩家列表, userId={userId}");

            ApiRequest playerListRequest = new ApiRequest
            {
                Command = "GetPlayersByUserId",
                GetPlayersByUserId = new GetPlayersByUserIdRequest
                {
                    UserId = userId
                }
            };
            NetworkClient.Instance.SendRequest(playerListRequest);
            
        }
        else
        {
            Debug.Log($"用户登录失败: {response.Error}");
            // 显示提示信息面板
            PromptMessage.Instance.ShowInfo(response.Message);
            // 完成后恢复按钮
            OKButton.interactable = true;
            step = 0;  // 失败后重置
        }
    }

    private void HandlePlayerListResponse(ApiResponse response)
    {
        if (response.Success)
        {
            Debug.Log($"[HandlePlayerListResponse] 查询成功, 该用户选择玩家...");
            players.AddRange(response.Session.Data.Players);

            // 跳转场景 【选择场景】
            SceneManager.LoadScene(1);

        }
        else
        {
            Debug.Log($"查询失败, 该用户下没有玩家, 加载创建角色界面...");
            // 跳转场景 【创建角色场景】
            SceneManager.LoadScene(2);
        }
    }

    // 打开 登陆界面
    public void showPanel()
    {
        gameObject.SetActive(true); // 显示该窗口
        cleanAll();
    }

    // 关闭 登录界面
    public void closePanel()
    {
        cleanAll();
        gameObject.SetActive(false); // 关闭该窗口
    }

    void cleanAll()
    {
        userNameInput.text = "";
        passwordInput.text = "";
        ErrorPrompt.Instance.CloseErrorPrompt();
    }

    // 验证表单是否可以提交
    private void ValidateForm()
    {
        // 表单有效标志，只有当所有验证通过时，表单才被认为有效
        bool isValid = isUserNameValid && isPasswordValid;
        OKButton.interactable = isValid;
        if (isValid)
        {
            ErrorPrompt.Instance.CloseErrorPrompt();
        }

    }
}