using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class ChatUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private string playerName = "Nguyen";

    [Header("Input")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;

    [Header("Messages")]
    [SerializeField] private Transform content;
    [SerializeField] private ChatMessageItem messagePrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Optional UI Parts")]
    [SerializeField] private GameObject header;
    [SerializeField] private GameObject inputArea;
    [SerializeField] private GameObject verticalScrollbar;
    [SerializeField] private RectTransform messageArea;

    [Header("Panel Hiding")]
    [Tooltip(
        "Có thể để trống. Code sẽ tự tìm và ẩn toàn bộ Image/RawImage " +
        "của panel chat, ngoại trừ hình nằm trong các tin nhắn."
    )]
    [SerializeField] private Graphic[] expandedOnlyGraphics;

    [SerializeField] private bool autoCollectPanelGraphics = true;

    [Tooltip(
        "Bật để vô hiệu Mask cũ khi chat thu gọn. " +
        "Rect Mask 2D vẫn hoạt động bình thường."
    )]
    [SerializeField] private bool disableLegacyMasksInCompact = true;

    [Header("Auto Compact")]
    [SerializeField, Min(1f)]
    private float autoCompactDelay = 30f;

    [SerializeField, Range(1, 20)]
    private int compactVisibleMessages = 6;

    [SerializeField]
    private bool expandMessageAreaInCompact;

    [SerializeField, Min(0f)]
    private float compactMargin = 8f;

    [Header("Storage")]
    [SerializeField, Min(1)]
    private int maxMessages = 50;

    private readonly Dictionary<Graphic, bool> originalGraphicStates = new();
    private readonly Dictionary<Mask, bool> originalMaskStates = new();

    private CanvasGroup rootCanvasGroup;
    private Vector2 expandedOffsetMin;
    private Vector2 expandedOffsetMax;

    private float lastActivityTime;
    private bool isCompact;
    private bool isSubscribed;
    private int lastSubmitFrame = -1;

    private Coroutine focusCoroutine;
    private Coroutine networkWaitCoroutine;

    private void Awake()
    {
        rootCanvasGroup = GetComponent<CanvasGroup>();

        ResolveOptionalReferences();
        SaveExpandedMessageArea();
        CachePanelGraphics();
        CacheLegacyMasks();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        sendButton.onClick.AddListener(SendCurrentMessage);
        messageInput.onSubmit.AddListener(OnInputSubmitted);
        messageInput.onSelect.AddListener(OnInputSelected);
        messageInput.onValueChanged.AddListener(OnInputChanged);

        SetCompactMode(false);

        networkWaitCoroutine = StartCoroutine(
            WaitForChatNetwork()
        );
    }

    private void Update()
    {
        bool canOpenWithEnter =
            Time.frameCount != lastSubmitFrame;

        if (canOpenWithEnter &&
            WasEnterPressed() &&
            !messageInput.isFocused)
        {
            OpenChat();
            return;
        }

        if (WasEscapePressed() && !isCompact)
        {
            messageInput.DeactivateInputField();
            SetCompactMode(true);
            return;
        }

        if (messageInput.isFocused)
        {
            RegisterActivity();
            return;
        }

        float inactiveTime =
            Time.unscaledTime - lastActivityTime;

        if (!isCompact &&
            inactiveTime >= autoCompactDelay)
        {
            SetCompactMode(true);
        }
    }

    private IEnumerator WaitForChatNetwork()
    {
        while (ChatNetwork.Instance == null)
        {
            yield return null;
        }

        ChatNetwork.Instance.OnMessageReceived += ReceiveMessage;
        isSubscribed = true;
        networkWaitCoroutine = null;
    }

    private void OnInputSubmitted(string value)
    {
        lastSubmitFrame = Time.frameCount;
        SendCurrentMessage();
    }

    private void OnInputSelected(string value)
    {
        RegisterActivity();
    }

    private void OnInputChanged(string value)
    {
        RegisterActivity();
    }

    private void SendCurrentMessage()
    {
        string message = messageInput.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            OpenChat();
            return;
        }

        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning(
                "Hãy bấm Host hoặc Client trước khi gửi tin nhắn.",
                this
            );

            OpenChat();
            return;
        }

        if (ChatNetwork.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy ChatNetwork trong Scene.",
                this
            );
            return;
        }

        ChatNetwork.Instance.SendMessage(
            playerName,
            message
        );

        messageInput.SetTextWithoutNotify(string.Empty);
        messageInput.DeactivateInputField();

        RegisterActivity();
    }

    private void ReceiveMessage(
        ulong senderId,
        string senderName,
        string message)
    {
        ChatMessageItem newItem = Instantiate(
            messagePrefab,
            content
        );

        bool isMine =
            NetworkManager.Singleton != null &&
            senderId == NetworkManager.Singleton.LocalClientId;

        newItem.Setup(
            senderName,
            message,
            isMine
        );

        newItem.SetCompactMode(isCompact);

        RemoveOldMessages();
        RefreshMessagePresentation();

        StartCoroutine(ScrollToBottom());

        // Tin mới khi đang compact chỉ hiện tên + nội dung,
        // không tự bung panel chat.
        lastActivityTime = Time.unscaledTime;
    }

    private void OpenChat()
    {
        SetCompactMode(false);

        if (focusCoroutine != null)
        {
            StopCoroutine(focusCoroutine);
        }

        focusCoroutine = StartCoroutine(
            FocusInputNextFrame()
        );
    }

    private IEnumerator FocusInputNextFrame()
    {
        yield return null;

        messageInput.ActivateInputField();
        messageInput.MoveTextEnd(false);

        focusCoroutine = null;
    }

    private void RegisterActivity()
    {
        lastActivityTime = Time.unscaledTime;
    }

    private void SetCompactMode(bool compact)
    {
        isCompact = compact;

        if (compact)
        {
            messageInput.DeactivateInputField();
        }

        SetObjectVisible(header, !compact);
        SetObjectVisible(inputArea, !compact);
        SetObjectVisible(verticalScrollbar, !compact);

        // Bảo hiểm trong trường hợp InputArea gắn sai hoặc để trống.
        SetObjectVisible(messageInput.gameObject, !compact);
        SetObjectVisible(sendButton.gameObject, !compact);

        SetPanelGraphicsVisible(!compact);
        SetLegacyMasksVisible(!compact);
        UpdateMessageArea(compact);

        // Chữ tin nhắn luôn hiện, nhưng panel không chặn chuột/gameplay.
        rootCanvasGroup.alpha = 1f;
        rootCanvasGroup.interactable = !compact;
        rootCanvasGroup.blocksRaycasts = !compact;

        RefreshMessagePresentation();
        lastActivityTime = Time.unscaledTime;
    }

    private void SetPanelGraphicsVisible(bool expanded)
    {
        foreach (KeyValuePair<Graphic, bool> entry in originalGraphicStates)
        {
            Graphic graphic = entry.Key;

            if (graphic == null)
            {
                continue;
            }

            graphic.enabled = expanded && entry.Value;
        }
    }

    private void SetLegacyMasksVisible(bool expanded)
    {
        if (!disableLegacyMasksInCompact)
        {
            return;
        }

        foreach (KeyValuePair<Mask, bool> entry in originalMaskStates)
        {
            Mask mask = entry.Key;

            if (mask == null)
            {
                continue;
            }

            mask.enabled = expanded && entry.Value;
        }
    }

    private void UpdateMessageArea(bool compact)
    {
        if (messageArea == null)
        {
            return;
        }

        if (compact && expandMessageAreaInCompact)
        {
            messageArea.offsetMin = new Vector2(
                compactMargin,
                compactMargin
            );

            messageArea.offsetMax = new Vector2(
                -compactMargin,
                -compactMargin
            );
            return;
        }

        messageArea.offsetMin = expandedOffsetMin;
        messageArea.offsetMax = expandedOffsetMax;
    }

    private void RefreshMessagePresentation()
    {
        int childCount = content.childCount;

        int firstVisibleMessage = Mathf.Max(
            0,
            childCount - compactVisibleMessages
        );

        for (int i = 0; i < childCount; i++)
        {
            GameObject child = content.GetChild(i).gameObject;

            bool shouldShow =
                !isCompact ||
                i >= firstVisibleMessage;

            child.SetActive(shouldShow);

            if (!shouldShow)
            {
                continue;
            }

            if (child.TryGetComponent(out ChatMessageItem item))
            {
                item.SetCompactMode(isCompact);
            }
        }
    }

    private void RemoveOldMessages()
    {
        while (content.childCount > maxMessages)
        {
            Transform oldestMessage = content.GetChild(0);
            oldestMessage.SetParent(null);
            Destroy(oldestMessage.gameObject);
        }
    }

    private IEnumerator ScrollToBottom()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (content is RectTransform contentRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ResolveOptionalReferences()
    {
        if (inputArea == null && messageInput != null)
        {
            inputArea = messageInput.transform.parent.gameObject;
        }

        if (verticalScrollbar == null &&
            scrollRect != null &&
            scrollRect.verticalScrollbar != null)
        {
            verticalScrollbar =
                scrollRect.verticalScrollbar.gameObject;
        }

        if (messageArea == null && scrollRect != null)
        {
            messageArea = scrollRect.transform.parent as RectTransform;
        }
    }

    private void SaveExpandedMessageArea()
    {
        if (messageArea == null)
        {
            return;
        }

        expandedOffsetMin = messageArea.offsetMin;
        expandedOffsetMax = messageArea.offsetMax;
    }

    private void CachePanelGraphics()
    {
        originalGraphicStates.Clear();

        if (expandedOnlyGraphics != null)
        {
            foreach (Graphic graphic in expandedOnlyGraphics)
            {
                AddPanelGraphic(graphic);
            }
        }

        if (!autoCollectPanelGraphics)
        {
            return;
        }

        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null)
            {
                continue;
            }

            // Chỉ tự thu thập hình nền, viền, icon và panel.
            // TMP_Text không bị đụng tới nên tên + nội dung vẫn hiện.
            if (graphic is not Image && graphic is not RawImage)
            {
                continue;
            }

            // Hình trong từng ChatMessageItem được chính item quản lý.
            if (content != null && graphic.transform.IsChildOf(content))
            {
                continue;
            }

            AddPanelGraphic(graphic);
        }
    }

    private void AddPanelGraphic(Graphic graphic)
    {
        if (graphic == null || originalGraphicStates.ContainsKey(graphic))
        {
            return;
        }

        originalGraphicStates.Add(graphic, graphic.enabled);
    }

    private void CacheLegacyMasks()
    {
        originalMaskStates.Clear();

        if (!disableLegacyMasksInCompact)
        {
            return;
        }

        Mask[] masks = GetComponentsInChildren<Mask>(true);

        foreach (Mask mask in masks)
        {
            if (mask == null)
            {
                continue;
            }

            originalMaskStates[mask] = mask.enabled;
        }
    }

    private static void SetObjectVisible(
        GameObject target,
        bool visible)
    {
        if (target != null && target.activeSelf != visible)
        {
            target.SetActive(visible);
        }
    }

    private bool ValidateReferences()
    {
        bool isValid =
            messageInput != null &&
            sendButton != null &&
            content != null &&
            messagePrefab != null &&
            scrollRect != null &&
            rootCanvasGroup != null;

        if (!isValid)
        {
            Debug.LogError(
                "ChatUI thiếu Message Input, Send Button, Content, " +
                "Message Prefab, Scroll Rect hoặc Canvas Group.",
                this
            );
        }

        return isValid;
    }

    private static bool WasEnterPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               (
                   Keyboard.current.enterKey.wasPressedThisFrame ||
                   Keyboard.current.numpadEnterKey.wasPressedThisFrame
               );
#else
        return Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
    }

    private static bool WasEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    private void OnDestroy()
    {
        if (focusCoroutine != null)
        {
            StopCoroutine(focusCoroutine);
        }

        if (networkWaitCoroutine != null)
        {
            StopCoroutine(networkWaitCoroutine);
        }

        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(SendCurrentMessage);
        }

        if (messageInput != null)
        {
            messageInput.onSubmit.RemoveListener(OnInputSubmitted);
            messageInput.onSelect.RemoveListener(OnInputSelected);
            messageInput.onValueChanged.RemoveListener(OnInputChanged);
        }

        if (isSubscribed && ChatNetwork.Instance != null)
        {
            ChatNetwork.Instance.OnMessageReceived -= ReceiveMessage;
        }
    }
}