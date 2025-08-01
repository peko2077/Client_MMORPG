using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MianManege : MonoBehaviour, IPointerClickHandler
{
    public static MianManege Instance;

    private Image registerIcon;
    private Image loginIcon;
    private Image settingsIcon;
    private Image signOutIcon;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("检测到多个MianManege实例！");
            Destroy(gameObject);  // 防止重复实例化
        }

        registerIcon = transform.Find("RegisterIcon").GetComponent<Image>();
        loginIcon = transform.Find("LoginIcon").GetComponent<Image>();
        signOutIcon = transform.Find("SignOutIcon").GetComponent<Image>();

        registerIcon.transform.gameObject.SetActive(false);
        loginIcon.transform.gameObject.SetActive(false);

        signOutIcon.transform.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    void Start()
    {
        bool existUserName = PlayerPrefs.HasKey("UserName");
        bool existPassword = PlayerPrefs.HasKey("Password");

        if (existUserName && existPassword)
        {
            // 本地已登录
            OpenSignOutIcon();
            StartManage.Instance.showPanel();
        }
        else
        {
            // 本地未登录
            OpenIconRegisterAndLoginIcon();
            UserRegister.Instance.showPanel();
            closePanel();
        }

    }

    public void showPanel()
    {
        gameObject.SetActive(true);
    }

    public void closePanel()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        string clickedObject = eventData.pointerCurrentRaycast.gameObject.transform.parent.name;
        // Debug.Log($"name = {clickedObject}");
        if (clickedObject == "RegisterIcon")
        {
            UserRegister.Instance.showPanel();
            closePanel();
        }
        else if (clickedObject == "LoginIcon")
        {
            UserLogin.Instance.showPanel();
            closePanel();
        }
        else if (clickedObject == "SignOutIcon")
        {
            PlayerPrefs.DeleteKey("UserId");    // 根据Key删除
            PlayerPrefs.DeleteKey("UserName");  // 根据Key删除
            PlayerPrefs.DeleteKey("Password");  // 根据Key删除

            OpenIconRegisterAndLoginIcon();

            CloseSignOutIcon();
            PromptMessage.Instance.ShowInfo("注销成功！");

            StartManage.Instance.closePanel();
        }

    }

    public void OpenIconRegisterAndLoginIcon()
    {
        registerIcon.transform.gameObject.SetActive(true);
        loginIcon.transform.gameObject.SetActive(true);
    }

    public void CloseIconRegisterAndLoginIcon()
    {
        registerIcon.transform.gameObject.SetActive(false);
        loginIcon.transform.gameObject.SetActive(false);
    }

    public void OpenSignOutIcon()
    {
        signOutIcon.transform.gameObject.SetActive(true);
    }

    public void CloseSignOutIcon()
    {
        signOutIcon.transform.gameObject.SetActive(false);
    }
}
