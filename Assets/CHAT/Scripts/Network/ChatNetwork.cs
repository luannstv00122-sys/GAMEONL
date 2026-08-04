using System;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject))]
public sealed class ChatNetwork : NetworkBehaviour
{
    public static ChatNetwork Instance { get; private set; }

    public event Action<ulong, string, string> OnMessageReceived;

    private const int MaxNameLength = 20;
    private const int MaxMessageLength = 150;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        // NetworkBehaviour cần chạy phần cleanup nội bộ.
        base.OnDestroy();
    }

    /// <summary>
    /// Được ChatUI gọi khi người chơi muốn gửi tin nhắn.
    /// </summary>
    public void SendMessage(
        string playerName,
        string message)
    {
        if (!IsSpawned)
        {
            Debug.LogWarning(
                "ChatNetwork chưa được NetworkManager spawn.",
                this
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        SendChatRpc(playerName, message);
    }

    /// <summary>
    /// Client gửi nội dung lên server.
    /// Mọi client đều được phép gọi RPC này.
    /// </summary>
    [Rpc(
        SendTo.Server,
        InvokePermission = RpcInvokePermission.Everyone
    )]
    private void SendChatRpc(
        string playerName,
        string message,
        RpcParams rpcParams = default)
    {
        ulong senderId =
            rpcParams.Receive.SenderClientId;

        string cleanPlayerName = CleanText(
            playerName,
            $"Player {senderId}",
            MaxNameLength
        );

        string cleanMessage = CleanText(
            message,
            string.Empty,
            MaxMessageLength
        );

        if (string.IsNullOrWhiteSpace(cleanMessage))
        {
            return;
        }

        BroadcastChatRpc(
            senderId,
            cleanPlayerName,
            cleanMessage
        );
    }

    /// <summary>
    /// Server phát tin nhắn đến Host và toàn bộ Client.
    /// Chỉ server được quyền gọi RPC này.
    /// </summary>
    [Rpc(
        SendTo.Everyone,
        InvokePermission = RpcInvokePermission.Server
    )]
    private void BroadcastChatRpc(
        ulong senderId,
        string playerName,
        string message)
    {
        OnMessageReceived?.Invoke(
            senderId,
            playerName,
            message
        );
    }

    /// <summary>
    /// Làm sạch chuỗi và giới hạn độ dài.
    /// </summary>
    private static string CleanText(
        string value,
        string fallback,
        int maxLength)
    {
        string result = string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();

        if (result.Length > maxLength)
        {
            result = result.Substring(
                0,
                maxLength
            );
        }

        return result;
    }
}