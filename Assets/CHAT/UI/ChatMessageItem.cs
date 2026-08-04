using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ChatMessageItem : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text messageText;

    [Header("Optional Visuals")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image accentBar;

    [Header("Normal Message")]
    [SerializeField] private Color normalNameColor =
        new Color32(86, 199, 255, 255);

    [SerializeField] private Color normalBackgroundColor =
        new Color32(17, 27, 41, 210);

    [Header("My Message")]
    [SerializeField] private Color myNameColor =
        new Color32(139, 224, 255, 255);

    [SerializeField] private Color myBackgroundColor =
        new Color32(18, 40, 58, 225);

    [Header("System Message")]
    [SerializeField] private Color systemNameColor =
        new Color32(84, 230, 157, 255);

    [SerializeField] private Color systemBackgroundColor =
        new Color32(27, 36, 48, 220);

    [Header("Common Text")]
    [SerializeField] private Color messageColor =
        new Color32(237, 245, 255, 255);

    [SerializeField] private Color timeColor =
        new Color32(109, 124, 145, 255);

    private readonly Dictionary<Graphic, bool> originalVisualStates = new();

    private Color expandedBackgroundColor;
    private GameObject obsoleteCompactBorder;

    private void Awake()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        Transform oldBorder = transform.Find("CompactBorder");

        if (oldBorder != null)
        {
            obsoleteCompactBorder = oldBorder.gameObject;
            obsoleteCompactBorder.SetActive(false);
        }

        CacheVisualGraphics();
        SetTextRaycastTargets(false);
    }

    /// <summary>
    /// Gán tên, thời gian, nội dung và kiểu hiển thị cho tin nhắn.
    /// </summary>
    public void Setup(
        string playerName,
        string message,
        bool isMine = false,
        bool isSystem = false)
    {
        if (!ValidateReferences())
        {
            return;
        }

        string cleanName = string.IsNullOrWhiteSpace(playerName)
            ? "Player"
            : playerName.Trim();

        string cleanMessage = string.IsNullOrWhiteSpace(message)
            ? "(Tin nhắn trống)"
            : message.Trim();

        nameText.richText = false;
        timeText.richText = false;
        messageText.richText = false;

        nameText.text = isSystem
            ? "SYSTEM"
            : cleanName;

        timeText.text = DateTime.Now.ToString("HH:mm");
        messageText.text = cleanMessage;

        ApplyStyle(isMine, isSystem);
        SetCompactMode(false);
        RebuildLayout();
    }

    /// <summary>
    /// Compact mode hoàn toàn trong suốt:
    /// chỉ giữ tên người chơi và nội dung tin nhắn.
    /// </summary>
    public void SetCompactMode(bool compact)
    {
        foreach (KeyValuePair<Graphic, bool> entry in originalVisualStates)
        {
            Graphic graphic = entry.Key;

            if (graphic == null)
            {
                continue;
            }

            graphic.enabled = !compact && entry.Value;
        }

        if (!compact && backgroundImage != null)
        {
            backgroundImage.color = expandedBackgroundColor;
        }

        if (obsoleteCompactBorder != null)
        {
            obsoleteCompactBorder.SetActive(false);
        }

        if (timeText != null)
        {
            timeText.gameObject.SetActive(!compact);
        }

        if (nameText != null)
        {
            nameText.gameObject.SetActive(true);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
        }

        RebuildLayout();
    }

    private void ApplyStyle(
        bool isMine,
        bool isSystem)
    {
        Color selectedNameColor;

        if (isSystem)
        {
            selectedNameColor = systemNameColor;
            expandedBackgroundColor = systemBackgroundColor;
        }
        else if (isMine)
        {
            selectedNameColor = myNameColor;
            expandedBackgroundColor = myBackgroundColor;
        }
        else
        {
            selectedNameColor = normalNameColor;
            expandedBackgroundColor = normalBackgroundColor;
        }

        nameText.color = selectedNameColor;
        timeText.color = timeColor;
        messageText.color = messageColor;

        if (backgroundImage != null)
        {
            backgroundImage.color = expandedBackgroundColor;
        }

        if (accentBar != null)
        {
            accentBar.color = selectedNameColor;
        }
    }

    private void CacheVisualGraphics()
    {
        originalVisualStates.Clear();

        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null)
            {
                continue;
            }

            // TMP_Text phải còn hiện khi compact.
            if (graphic is TMP_Text)
            {
                continue;
            }

            // Chỉ quản lý hình nền, viền, accent và icon.
            if (graphic is not Image && graphic is not RawImage)
            {
                continue;
            }

            originalVisualStates[graphic] = graphic.enabled;
            graphic.raycastTarget = false;
        }
    }

    private void RebuildLayout()
    {
        if (transform is not RectTransform rectTransform)
        {
            return;
        }

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

        if (rectTransform.parent is RectTransform parentRect)
        {
            LayoutRebuilder.MarkLayoutForRebuild(parentRect);
        }
    }

    private void SetTextRaycastTargets(bool enabled)
    {
        if (nameText != null)
        {
            nameText.raycastTarget = enabled;
        }

        if (timeText != null)
        {
            timeText.raycastTarget = enabled;
        }

        if (messageText != null)
        {
            messageText.raycastTarget = enabled;
        }
    }

    private bool ValidateReferences()
    {
        bool isValid =
            nameText != null &&
            timeText != null &&
            messageText != null;

        if (!isValid)
        {
            Debug.LogError(
                "ChatMessageItem thiếu Name Text, Time Text hoặc Message Text.",
                this
            );
        }

        return isValid;
    }
}