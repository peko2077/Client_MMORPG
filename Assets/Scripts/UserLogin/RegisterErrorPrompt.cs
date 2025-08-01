using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RegisterErrorPrompt : MonoBehaviour
{
    public static RegisterErrorPrompt Instance;

    private TextMeshProUGUI errorPrompt;

    void Awake()
    {
        Instance = this;
        errorPrompt = GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void ShowErrorPrompt(string message)
    {
        gameObject.SetActive(true);
        errorPrompt.text = message;
    }

    public void CloseErrorPrompt()
    {
        gameObject.SetActive(false);
    }
}
