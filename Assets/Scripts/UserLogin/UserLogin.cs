using System;
using System.Collections;
using Google.Protobuf;
using Mymmorpg;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserLogin : MonoBehaviour
{
    public static UserLogin Instance;

    private int step = 0;

    // 用于延迟验证的标志
    private bool isUserNameValid = false;  // 用户名是否有效
    private bool isPasswordValid = false;  // 密码是否有效

    // 定义 TMP_InputField 类型的变量，分别用于用户名和密码输入框
    private TMP_InputField userNameInput;
    private TMP_InputField passwordInput;

    // 定义一个 Button 变量，用于确认按钮
    private Button OKButton;
    private Button backButton;

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

        // 第一步：发送 AddPlayer 请求
        ApiRequest request = new ApiRequest
        {
            Command = "LoginUser",
            LoginUser = new LoginUserRequest
            {
                UserName = userName,
                Password = password,
            }
        };

        step = 1; // 标记当前是 用户登录 阶段

        NetworkClient.Instance.SendRequest(request); // 发送请求
    }

    private void OnApiResponse(ApiResponse response)
    {
        Debug.Log("收到服务器响应: " + response.Message);

        if (step == 1)
        {
            if (response.Success)
            {
                Debug.Log($"用户登录成功: {response.User.UserName}, ID: {response.User.UserId}");
                // 显示提示信息面板
                PromptMessage.Instance.ShowInfo(response.Message);
                step = 2;  // 成功后进入下一阶段
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