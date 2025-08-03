using System.Collections;
using System.Collections.Generic;
using Mymmorpg;
using TMPro;
using UnityEngine;

public class DropdownPlayers : MonoBehaviour
{
    public static DropdownPlayers Instance;

    private List<Player> currentPlayers;

    private TMP_Dropdown dropdownPlayers;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("检测到多个 DropdownPlayers 实例！");
            Destroy(gameObject);  // 防止重复实例化
        }

        dropdownPlayers = GetComponent<TMP_Dropdown>();
    }

    void Start()
    {
        currentPlayers = UserLogin.Instance.Players;

        // 清空已有选项（避免重复）
        dropdownPlayers.ClearOptions();

        // 创建一个字符串列表，存放所有的 PlayerName
        List<string> playerNames = new List<string>();

        foreach (Player player in currentPlayers)
        {
            playerNames.Add(player.PlayerName);
            Debug.Log($"[DropdownPlayers]: playerId = {player.PlayerId}, playerName = {player.PlayerName}");
        }

        // 添加到 dropdown 中
        dropdownPlayers.AddOptions(playerNames);

        // 添加事件监听器
        dropdownPlayers.onValueChanged.AddListener(OnDropdownValueChanged);

    }

    void Update()
    {

    }

    void OnDropdownValueChanged(int index)
    {
        if (index >= 0 && index < currentPlayers.Count)
        {
            Player selectedPlayer = currentPlayers[index];
            Debug.Log($"[DropdownPlayers] 选中玩家: PlayerName = {selectedPlayer.PlayerName}, PlayerId = {selectedPlayer.PlayerId}");
        }
    }
}