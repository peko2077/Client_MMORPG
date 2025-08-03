using System.Collections.Generic;
using Mymmorpg;
using TMPro;
using UnityEngine;

public class SelectPlayerManage : MonoBehaviour
{
    public static SelectPlayerManage Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("检测到多个 SelectPlayerManage 实例！");
            Destroy(gameObject);  // 防止重复实例化
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }
}