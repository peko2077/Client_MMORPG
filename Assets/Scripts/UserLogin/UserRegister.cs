using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Mymmorpg;

public class UserRegister : MonoBehaviour, IPointerClickHandler
{
    // 静态实例，使得其他类可以访问该类的实例
    public static UserRegister Instance;

    private int step = 0;

    // 输入框组件
    private TMP_InputField userNameInput;          // 用户名输入框
    private TMP_InputField passwordInput;          // 密码输入框
    private TMP_InputField secondaryPasswordInput; // 确认密码输入框

    // 按钮组件
    private Button confirmButton;  // 确认按钮
    private Button backButton;     // 返回按钮

    // 用于延迟验证的标志
    private bool isUserNameValid = false;  // 用户名是否有效
    private bool isPasswordValid = false;  // 密码是否有效
    private bool isSecondaryPasswordValid = false;  // 确认密码是否有效

    public bool isPanelShow = false;

    // Awake 在脚本实例化时调用，用于初始化
    void Awake()
    {
        Instance = this;  // 设置单例实例，其他类可以通过 UserRegister.Instance 访问

        // 获取各个输入框组件
        userNameInput = transform.Find("UserNameInput").GetComponent<TMP_InputField>();
        passwordInput = transform.Find("PasswordInput").GetComponent<TMP_InputField>();
        secondaryPasswordInput = transform.Find("SecondaryPasswordInput").GetComponent<TMP_InputField>();

        // 获取按钮组件
        confirmButton = transform.Find("ConfirmButton").GetComponent<Button>();
        backButton = transform.Find("BackButton").GetComponent<Button>();

        // 默认情况下，注册面板是关闭的
        gameObject.SetActive(false);

        // 初始时禁用确认按钮（输入不完整时不允许提交）
        confirmButton.interactable = false;
    }

    // Start 是脚本初始化后，执行的第一个函数，常用于事件绑定
    void Start()
    {
        // 监听按钮点击事件
        confirmButton.onClick.AddListener(OnClickOKButton);  // 点击确认按钮时触发
        backButton.onClick.AddListener(OnClickBackButton);  // 点击返回按钮时触发

        // 监听输入框完成编辑事件（onEndEdit），用户完成输入后触发
        userNameInput.onEndEdit.AddListener(OnUserNameEditEnd);
        passwordInput.onEndEdit.AddListener(OnPasswordEditEnd);
        secondaryPasswordInput.onEndEdit.AddListener(OnSecondaryPasswordEditEnd);

        // 订阅响应事件
        NetworkClient.Instance.OnApiResponse += OnApiResponse;
    }

    void OnDestroy()
    {
        // 解绑事件，避免重复触发或内存泄漏
        if (NetworkClient.Instance != null)
            NetworkClient.Instance.OnApiResponse -= OnApiResponse;
    }

    // 用户名输入框编辑完成后触发
    void OnUserNameEditEnd(string input)
    {
        // 判断用户名是否符合要求（至少6个字符）
        if (input.Length < 6)
        {
            // 显示错误提示
            RegisterErrorPrompt.Instance.ShowErrorPrompt("用户名长度不能低于6个字符");
            isUserNameValid = false;  // 设置用户名验证为无效
        }
        else
        {
            // 清除错误提示
            RegisterErrorPrompt.Instance.CloseErrorPrompt();
            isUserNameValid = true;  // 设置用户名验证为有效
        }

        // 调用表单验证，判断确认按钮是否启用
        ValidateForm();
    }

    // 密码输入框编辑完成后触发
    void OnPasswordEditEnd(string input)
    {
        // 判断密码是否符合要求（至少8个字符）
        if (input.Length < 8)
        {
            // 显示错误提示
            RegisterErrorPrompt.Instance.ShowErrorPrompt("密码长度不能低于8个字符");
            isPasswordValid = false;  // 设置密码验证为无效
        }
        else
        {
            // 清除错误提示
            RegisterErrorPrompt.Instance.CloseErrorPrompt();
            isPasswordValid = true;  // 设置密码验证为有效
        }

        // 调用表单验证，判断确认按钮是否启用
        ValidateForm();
    }

    // 确认密码输入框编辑完成后触发
    void OnSecondaryPasswordEditEnd(string input)
    {
        // 判断确认密码是否与密码一致
        if (input != passwordInput.text)
        {
            // 显示错误提示
            RegisterErrorPrompt.Instance.ShowErrorPrompt("两次密码不一致！");
            isSecondaryPasswordValid = false;  // 设置确认密码验证为无效
        }
        else
        {
            // 清除错误提示
            RegisterErrorPrompt.Instance.CloseErrorPrompt();
            isSecondaryPasswordValid = true;  // 设置确认密码验证为有效
        }

        // 调用表单验证，判断确认按钮是否启用
        ValidateForm();
    }

    // 返回按钮点击事件
    void OnClickBackButton()
    {
        // 显示主面板
        MianManege.Instance.showPanel();
        // 关闭当前面板
        closePanel();
    }

    // 确认按钮点击事件
    void OnClickOKButton()
    {
        // 提交之前，关闭任何错误提示
        RegisterErrorPrompt.Instance.CloseErrorPrompt();

        confirmButton.interactable = false;

        // 获取用户输入的用户名和密码
        string userName = userNameInput.text;
        string password = passwordInput.text;

        // 第一步：发送 AddPlayer 请求
        ApiRequest request = new ApiRequest
        {
            Command = "AddUser",
            AddUser = new AddUserRequest
            {
                UserName = userName,
                Password = password,
            }
        };

        step = 1; // 标记当前是 创建用户 阶段

        NetworkClient.Instance.SendRequest(request); // 发送请求
    }

    private void OnApiResponse(ApiResponse response)
    {
        Debug.Log("收到服务器响应: " + response.Message);

        if (step == 1)
        {
            if (response.Success)
            {
                Debug.Log($"用户创建成功: {response.User.UserName}, ID: {response.User.UserId}");
                // 显示提示信息面板
                PromptMessage.Instance.ShowInfo(response.Message);
                step = 2;  // 成功后进入下一阶段
            }
            else
            {
                Debug.LogWarning($"用户创建失败: {response.Error}");
                // 显示提示信息面板
                PromptMessage.Instance.ShowInfo(response.Message);

                // 完成后恢复按钮
                confirmButton.interactable = true;
                step = 0;  // 失败后重置
            }
        }
    }

    // 显示面板
    public void showPanel()
    {
        gameObject.SetActive(true);  // 激活面板
        cleanAll();  // 清空所有输入框和提示错误
    }

    // 关闭面板
    public void closePanel()
    {
        // 禁用按钮
        if (confirmButton.interactable != false)
        {
            confirmButton.interactable = false;
        }
        cleanAll();  // 清空所有输入框和提示错误
        gameObject.SetActive(false);  // 隐藏面板
    }

    // 点击事件处理（例如点击“去登录”按钮）
    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果点击了"GoLogin"按钮，则跳转到登录界面
        if (eventData.pointerCurrentRaycast.gameObject.name == "GoLogin")
        {
            UserLogin.Instance.showPanel();
            closePanel();
        }
    }

    // 清空所有输入框和错误提示
    void cleanAll()
    {
        // 清空输入框的文本内容
        userNameInput.text = "";
        passwordInput.text = "";
        secondaryPasswordInput.text = "";

        // 关闭任何错误提示
        RegisterErrorPrompt.Instance.CloseErrorPrompt();
    }

    // 验证表单是否可以提交
    private void ValidateForm()
    {
        // 表单有效标志，只有当所有验证通过时，表单才被认为有效
        bool isValid = isUserNameValid && isPasswordValid && isSecondaryPasswordValid;

        // 如果表单有效，启用确认按钮；否则禁用确认按钮
        confirmButton.interactable = isValid;
    }
}