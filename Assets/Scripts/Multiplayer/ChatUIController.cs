using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChatUIController : MonoBehaviour
{
    public static ChatUIController Instance { get; private set; }

    public static bool IsTyping =>
        Instance != null && Instance.isTyping;

    [Header("UI")]
    [SerializeField] private GameObject chatInputPanel;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private GameObject messageHistoryScrollView;

    private NetworkChat networkChat;
    private bool isTyping;

    private void Awake()
{
    Instance = this;

    // Lịch sử chat luôn hiện.
    if (messageHistoryScrollView != null)
        messageHistoryScrollView.SetActive(true);

    // Ô nhập chỉ hiện khi nhấn Enter.
    if (chatInputPanel != null)
        chatInputPanel.SetActive(false);

    if (sendButton != null)
        sendButton.onClick.AddListener(SendCurrentMessage);
}

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        bool enterPressed =
            keyboard.enterKey.wasPressedThisFrame ||
            keyboard.numpadEnterKey.wasPressedThisFrame;

        // Enter lần đầu: mở chat.
        if (!isTyping && enterPressed)
        {
            OpenChat();
            return;
        }

        if (!isTyping)
            return;

        // Chuột phải: đóng chat, không gửi.
        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            CloseChat();
            return;
        }

        // Enter khi đang nhập: gửi.
        if (enterPressed)
        {
            SendCurrentMessage();
        }
    }

    public void SetNetworkChat(NetworkChat chat)
    {
        networkChat = chat;
    }

    private void OpenChat()
{
    isTyping = true;

    if (messageHistoryScrollView != null)
        messageHistoryScrollView.SetActive(true);

    if (chatInputPanel != null)
        chatInputPanel.SetActive(true);

    if (messageInput != null)
    {
        messageInput.text = "";
        messageInput.Select();
        messageInput.ActivateInputField();
    }

    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
}

    public void SendCurrentMessage()
{
    Debug.Log(
        $"Send được gọi | IsTyping={isTyping} | " +
        $"NetworkChat={(networkChat != null)} | " +
        $"Text={messageInput?.text}"
    );

    if (!isTyping || messageInput == null)
        return;

    string message = messageInput.text.Trim();

    if (!string.IsNullOrWhiteSpace(message) &&
        networkChat != null)
    {
        networkChat.SendMessageFromLocalUI(message);
    }

    CloseChat();
    
}

    private void CloseChat()
{
    isTyping = false;

    if (messageInput != null)
    {
        messageInput.text = "";
        messageInput.DeactivateInputField();
    }

    // Chỉ ẩn ô nhập.
    if (chatInputPanel != null)
        chatInputPanel.SetActive(false);

    // Không tắt lịch sử chat.
    if (messageHistoryScrollView != null)
        messageHistoryScrollView.SetActive(true);

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
}

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SendCurrentMessage);

        if (Instance == this)
            Instance = null;
    }
    
}