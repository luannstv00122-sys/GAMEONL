using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Room UI")]
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text playerListText;
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private TMP_Text statusText;

    [Header("Character UI - Optional")]
    [Tooltip("Có thể để trống nếu CharacterSelectionUI đang quản lý phần chọn nhân vật.")]
    [SerializeField] private Button[] characterButtons;

    [Tooltip("Có thể để trống nếu CharacterSelectionUI đang quản lý phần chọn nhân vật.")]
    [SerializeField] private TMP_Text[] characterStatusTexts;

    [Header("Buttons")]
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonText;
    [SerializeField] private Button startGameButton;

    [Header("Panels")]
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private GameObject characterPanel;

    [Header("Map Names")]
    [SerializeField] private string[] mapNames =
    {
        "Maintenance Deck",
        "Cargo Bay 04",
        "Hangar G8",
        "Sci-Fi Corridor Alpha"
    };

    private NetworkRunner _runner;
    private LobbyState _lobbyState;

    private void Update()
{
    FindNetworkObjects();

    if (_runner == null ||
        !_runner.IsRunning ||
        _lobbyState == null ||
        _lobbyState.Object == null ||
        !_lobbyState.Object.IsValid)
    {
        return;
    }

    UpdateUI();
}

    private void FindNetworkObjects()
    {
        if (_runner == null)
            _runner = FindFirstObjectByType<NetworkRunner>();

        if (_lobbyState == null)
            _lobbyState = FindFirstObjectByType<LobbyState>();
    }

    private void UpdateUI()
    {
        UpdatePlayerCount();
        UpdatePlayerList();
        UpdateMapUI();
        UpdateCharacterUI();
        UpdateReadyUI();
        UpdateStartButton();
    }

    private void UpdatePlayerCount()
    {
        // Sửa lỗi NullReference ở dòng playerCountText.text.
        if (playerCountText == null)
            return;

        int count = 0;

        foreach (PlayerRef player in _runner.ActivePlayers)
            count++;

        playerCountText.text = $"{count}/3";
    }

    private void UpdatePlayerList()
    {
        if (playerListText == null)
            return;

        string result = "";

        foreach (PlayerRef player in _runner.ActivePlayers)
        {
            int characterIndex = _lobbyState.GetCharacterIndex(player);

            

            string hostStatus =
                player == _runner.LocalPlayer && _runner.IsServer
                    ? " [Morant Việt]"
                    : "";

            result +=
                $"Player {player.PlayerId}{hostStatus}" ;
        }

        playerListText.text = result;
    }

    private void UpdateMapUI()
    {
        if (mapNameText == null)
            return;

        int mapIndex = _lobbyState.SelectedMap;

        if (mapIndex >= 0 && mapIndex < mapNames.Length)
            mapNameText.text = "" + mapNames[mapIndex];
        else
            mapNameText.text = "Chưa chọn";
    }

    private void UpdateCharacterUI()
    {
        // Nếu bạn đã dùng CharacterSelectionUI riêng thì có thể để hai mảng này trống.
        if (characterButtons == null || characterButtons.Length == 0)
            return;

        PlayerRef localPlayer = _runner.LocalPlayer;
        bool localReady = _lobbyState.IsPlayerReady(localPlayer);

        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i] == null)
                continue;

            PlayerRef owner = _lobbyState.GetCharacterOwner(i);

            bool empty = owner.IsNone;
            bool mine = owner == localPlayer;

            characterButtons[i].interactable =
                !localReady && (empty || mine);

            if (characterStatusTexts == null ||
                i >= characterStatusTexts.Length ||
                characterStatusTexts[i] == null)
            {
                continue;
            }

            if (empty)
                characterStatusTexts[i].text = "Có thể chọn";
            else if (mine)
                characterStatusTexts[i].text = "Bạn đã chọn";
            else
                characterStatusTexts[i].text =
                    $"Player {owner.PlayerId} đã chọn";
        }
    }

    private void UpdateReadyUI()
    {
        if (readyButton == null)
            return;

        bool isCharacterStage =
            _lobbyState.CurrentStage == LobbyStage.CharacterSelection;

        readyButton.gameObject.SetActive(isCharacterStage);

        if (!isCharacterStage)
            return;

        PlayerRef localPlayer = _runner.LocalPlayer;

        bool hasCharacter =
            _lobbyState.GetCharacterIndex(localPlayer) >= 0;

        bool ready =
            _lobbyState.IsPlayerReady(localPlayer);

        readyButton.interactable = hasCharacter;

        if (readyButtonText != null)
            readyButtonText.text =
                ready ? "Cancel Ready" : "Ready";
    }

    private void UpdateStartButton()
    {
        if (startGameButton == null)
            return;

        bool isCharacterStage =
            _lobbyState.CurrentStage == LobbyStage.CharacterSelection;

        bool shouldShow =
            _runner.IsServer &&
            isCharacterStage;

        startGameButton.gameObject.SetActive(shouldShow);

        startGameButton.interactable =
            shouldShow &&
            _lobbyState.CanStartGame();
    }

    // =========================
    // MAP SELECTION
    // =========================

    public void SelectMap0() => SelectMap(0);
    public void SelectMap1() => SelectMap(1);
    public void SelectMap2() => SelectMap(2);
    public void SelectMap3() => SelectMap(3);

    private void SelectMap(int mapIndex)
    {
        FindNetworkObjects();

        if (_lobbyState == null)
        {
            Debug.LogError("Không tìm thấy LobbyState.");
            return;
        }

        if (_runner == null || !_runner.IsRunning)
        {
            Debug.LogError("Không tìm thấy NetworkRunner đang hoạt động.");
            return;
        }

        if (!_runner.IsServer)
        {
            Debug.LogWarning("Chỉ Host mới được chọn map.");
            return;
        }

        _lobbyState.RequestSelectMap(mapIndex);
    }

    public void OpenCharacterPanel()
    {
        FindNetworkObjects();

        if (_lobbyState == null)
        {
            Debug.LogError("Không tìm thấy LobbyState.");
            return;
        }

        if (!_lobbyState.HasSelectedMap())
        {
            Debug.LogWarning("Hãy chọn một map trước.");
            return;
        }

        if (mapSelectionPanel != null)
            mapSelectionPanel.SetActive(false);

        if (characterPanel != null)
            characterPanel.SetActive(true);
    }

    // =========================
    // CHARACTER - dùng khi cần
    // =========================

    public void SelectCharacter0() => SelectCharacter(0);
    public void SelectCharacter1() => SelectCharacter(1);
    public void SelectCharacter2() => SelectCharacter(2);
    public void SelectCharacter3() => SelectCharacter(3);
    public void SelectCharacter4() => SelectCharacter(4);
    public void SelectCharacter5() => SelectCharacter(5);

    private void SelectCharacter(int index)
    {
        FindNetworkObjects();

        if (_lobbyState == null)
            return;

        _lobbyState.RequestCharacter(index);
    }

    // =========================
    // READY / START
    // =========================

    public void ToggleReady()
{
    Debug.Log("Đã bấm Ready");

    FindNetworkObjects();

    if (_lobbyState == null)
    {
        Debug.LogError("Không tìm thấy LobbyState.");
        return;
    }

    _lobbyState.RequestToggleReady();
}

    public void StartGame()
    {
        FindNetworkObjects();

        if (_lobbyState == null)
            return;

        _lobbyState.RequestStartGame();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}