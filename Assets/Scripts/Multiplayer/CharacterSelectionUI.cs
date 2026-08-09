using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Preview")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text characterNameText;

    [Header("Character Data")]
    [SerializeField] private Sprite[] characterSprites;
    [SerializeField] private string[] characterNames;

    [Header("Thumbnail UI")]
    [SerializeField] private Image[] thumbnailImages;
    [SerializeField] private Button[] thumbnailButtons;

    [Header("Buttons")]
    [SerializeField] private Button chooseButton;
    [SerializeField] private Button cancelButton;

    [Header("Visual Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float selectedDarkness = 0.18f;

    private NetworkRunner runner;
    private LobbyState lobbyState;
    private int previewIndex;

    private void Start()
    {
        previewIndex = 0;
        UpdatePreview();
    }

    private void Update()
{
    FindNetworkReferences();

    if (runner == null ||
        lobbyState == null ||
        lobbyState.Object == null ||
        !lobbyState.Object.IsValid)
    {
        return;
    }

    UpdateCharacterThumbnails();
    UpdateChooseButton();
}

    private void FindNetworkReferences()
    {
        if (runner == null)
            runner = FindFirstObjectByType<NetworkRunner>();

        if (lobbyState == null)
            lobbyState = FindFirstObjectByType<LobbyState>();
    }

    public void NextCharacter()
    {
        if (characterSprites == null || characterSprites.Length == 0)
            return;

        previewIndex++;

        if (previewIndex >= characterSprites.Length)
            previewIndex = 0;

        UpdatePreview();
    }

    public void PreviousCharacter()
    {
        if (characterSprites == null || characterSprites.Length == 0)
            return;

        previewIndex--;

        if (previewIndex < 0)
            previewIndex = characterSprites.Length - 1;

        UpdatePreview();
    }

    public void ChooseCharacter()
    {
        Debug.Log("Đã bấm Choose.");

        FindNetworkReferences();

        if (runner == null)
        {
            Debug.LogError("Không tìm thấy NetworkRunner.");
            return;
        }

        if (lobbyState == null)
        {
            Debug.LogError("Không tìm thấy LobbyState.");
            return;
        }

        if (characterSprites == null ||
            previewIndex < 0 ||
            previewIndex >= characterSprites.Length)
        {
            Debug.LogError("Character index không hợp lệ: " + previewIndex);
            return;
        }

        PlayerRef owner =
            lobbyState.GetCharacterOwner(previewIndex);

        bool available =
            owner.IsNone ||
            owner == runner.LocalPlayer;

        if (!available)
        {
            Debug.LogWarning("Nhân vật này đã được người khác chọn.");
            return;
        }

        Debug.Log(
            $"Player {runner.LocalPlayer.PlayerId} chọn Character {previewIndex}"
        );

        lobbyState.RequestCharacter(previewIndex);
    }

    public void CancelCharacter()
    {
        Debug.Log("Đã bấm Cancel.");

        FindNetworkReferences();

        if (runner == null)
        {
            Debug.LogError("Không tìm thấy NetworkRunner.");
            return;
        }

        if (lobbyState == null)
        {
            Debug.LogError("Không tìm thấy LobbyState.");
            return;
        }

        int myCharacter =
            lobbyState.GetCharacterIndex(runner.LocalPlayer);

        if (myCharacter < 0)
        {
            Debug.LogWarning("Bạn chưa chọn nhân vật để hủy.");
            return;
        }

        lobbyState.RequestCancelCharacter();
    }

    public void SelectThumbnail(int index)
    {
        if (characterSprites == null ||
            index < 0 ||
            index >= characterSprites.Length)
        {
            return;
        }

        previewIndex = index;
        UpdatePreview();
    }

    public void SelectJan() => SelectThumbnail(0);
    public void SelectEris() => SelectThumbnail(1);
    public void SelectVan() => SelectThumbnail(2);
    public void SelectKiller() => SelectThumbnail(3);
    public void SelectEkko() => SelectThumbnail(4);
    public void SelectJin() => SelectThumbnail(5);

    private void UpdatePreview()
    {
        if (characterSprites != null &&
            previewImage != null &&
            previewIndex >= 0 &&
            previewIndex < characterSprites.Length)
        {
            previewImage.sprite = characterSprites[previewIndex];
        }

        if (characterNames != null &&
            characterNameText != null &&
            previewIndex >= 0 &&
            previewIndex < characterNames.Length)
        {
            characterNameText.text = characterNames[previewIndex];
        }
    }

    private void UpdateCharacterThumbnails()
    {
        if (thumbnailImages == null || thumbnailButtons == null)
            return;

        int count = Mathf.Min(
            thumbnailImages.Length,
            thumbnailButtons.Length
        );

        for (int i = 0; i < count; i++)
        {
            if (thumbnailImages[i] == null ||
                thumbnailButtons[i] == null)
            {
                continue;
            }

            PlayerRef owner =
                lobbyState.GetCharacterOwner(i);

            bool hasOwner = !owner.IsNone;
            bool selectedByMe =
                hasOwner && owner == runner.LocalPlayer;

            thumbnailImages[i].color =
                hasOwner
                    ? new Color(
                        selectedDarkness,
                        selectedDarkness,
                        selectedDarkness,
                        1f
                    )
                    : Color.white;

            // Người khác chọn thì khóa nút.
            thumbnailButtons[i].interactable =
                !hasOwner || selectedByMe;
        }
    }

    private void UpdateChooseButton()
    {
        if (chooseButton == null || cancelButton == null)
            return;

        if (characterSprites == null ||
            previewIndex < 0 ||
            previewIndex >= characterSprites.Length)
        {
            chooseButton.interactable = false;
            cancelButton.interactable = false;
            return;
        }

        PlayerRef owner =
            lobbyState.GetCharacterOwner(previewIndex);

        bool available =
            owner.IsNone ||
            owner == runner.LocalPlayer;

        chooseButton.interactable = available;

        int myCharacter =
            lobbyState.GetCharacterIndex(runner.LocalPlayer);

        cancelButton.interactable = myCharacter >= 0;
    }
}