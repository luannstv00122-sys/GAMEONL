using Fusion;
using TMPro;
using UnityEngine;

public class NetworkChat : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxMessageLength = 80;

    private TMP_Text messageHistory;
    private NetworkPlayerInfo playerInfo;

    private static readonly string[] CharacterNames =
    {
        "Jan",
        "Eris",
        "Van",
        "Killer",
        "Ekko",
        "Jin"
    };

    private void Awake()
    {
        FindPlayerInfo();
    }

    public override void Spawned()
    {
        FindMessageHistory();

        // Chỉ nhân vật local mới được nối với UI nhập chat.
        if (!Object.HasInputAuthority)
            return;

        ChatUIController chatUI =
            FindFirstObjectByType<ChatUIController>();

        if (chatUI == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy ChatUIController trong scene."
            );
            return;
        }

        chatUI.SetNetworkChat(this);

        Debug.Log(
            $"{name}: Đã nối NetworkChat local. " +
            $"InputAuthority={Object.InputAuthority.PlayerId}, " +
            $"CharacterIndex={GetCharacterIndex()}"
        );
    }

    public void SendMessageFromLocalUI(string message)
    {
        if (!Object.HasInputAuthority)
            return;

        if (string.IsNullOrWhiteSpace(message))
            return;

        message = message.Trim();

        if (message.Length > maxMessageLength)
            message = message.Substring(0, maxMessageLength);

        RPC_SendChat(
            GetLocalCharacterName(),
            message
        );
    }

    private void FindPlayerInfo()
    {
        if (playerInfo != null)
            return;

        playerInfo = GetComponent<NetworkPlayerInfo>();

        if (playerInfo == null)
            playerInfo = GetComponentInParent<NetworkPlayerInfo>();

        if (playerInfo == null)
            playerInfo = GetComponentInChildren<NetworkPlayerInfo>();
    }

    private int GetCharacterIndex()
    {
        FindPlayerInfo();

        return playerInfo != null
            ? playerInfo.CharacterIndex
            : -1;
    }

    private string GetLocalCharacterName()
    {
        int characterIndex = GetCharacterIndex();

        if (characterIndex >= 0 &&
            characterIndex < CharacterNames.Length)
        {
            return CharacterNames[characterIndex];
        }

        Debug.LogWarning(
            $"{name}: CharacterIndex không hợp lệ: {characterIndex}"
        );

        return $"Player {Object.InputAuthority.PlayerId}";
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SendChat(
        string characterName,
        string message,
        RpcInfo info = default)
    {
        FindMessageHistory();

        if (messageHistory == null)
        {
            Debug.LogError("Không tìm thấy MessageHistory.");
            return;
        }

        messageHistory.text +=
            $"<b>{characterName}:</b> {message}\n";
    }

    private void FindMessageHistory()
    {
        if (messageHistory != null)
            return;

        GameObject historyObject =
            GameObject.Find("MessageHistory");

        if (historyObject == null)
            return;

        messageHistory =
            historyObject.GetComponent<TMP_Text>();
    }
}