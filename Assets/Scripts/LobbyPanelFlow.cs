using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyPanelFlow : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private GameObject characterSelectionPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Room UI")]
    [SerializeField] private GameObject hostButton;
    [SerializeField] private GameObject joinButton;
    [SerializeField] private Button roomNextButton;
    [SerializeField] private Button leaveButton;

    private NetworkRunner runner;
    private LobbyState lobbyState;

    private void Start()
    {
        ShowOnly(roomPanel);
        SetConnectedUI(false);
    }

    private void Update()
{
    FindNetworkObjects();

    bool connected =
        runner != null &&
        runner.IsRunning &&
        lobbyState != null &&
        lobbyState.Object != null &&
        lobbyState.Object.IsValid;

    SetConnectedUI(connected);

    if (!connected)
        return;

    UpdatePanelsFromNetwork();
    UpdateHostUI();
}
    private void FindNetworkObjects()
    {
        if (runner == null)
            runner = FindFirstObjectByType<NetworkRunner>();

        if (lobbyState == null)
            lobbyState = FindFirstObjectByType<LobbyState>();
    }

    private void SetConnectedUI(bool connected)
    {
        if (hostButton != null)
            hostButton.SetActive(!connected);

        if (joinButton != null)
            joinButton.SetActive(!connected);

        if (leaveButton != null)
            leaveButton.interactable = connected;

        if (roomNextButton != null)
        {
            bool isHost =
                connected &&
                runner != null &&
                runner.IsServer;

            roomNextButton.gameObject.SetActive(isHost);
            roomNextButton.interactable = isHost;
        }
    }

    private void UpdatePanelsFromNetwork()
    {
        switch (lobbyState.CurrentStage)
        {
            case LobbyStage.Room:
                ShowOnly(roomPanel);
                break;

            case LobbyStage.MapSelection:
                ShowOnly(mapSelectionPanel);
                break;

            case LobbyStage.CharacterSelection:
                ShowOnly(characterSelectionPanel);
                break;

            case LobbyStage.Loading:
                ShowOnly(loadingPanel);
                break;
        }
    }

    private void UpdateHostUI()
    {
        if (roomNextButton != null)
            roomNextButton.interactable = runner.IsServer;
    }

    private void ShowOnly(GameObject panelToShow)
    {
        SetPanel(roomPanel, panelToShow);
        SetPanel(mapSelectionPanel, panelToShow);
        SetPanel(characterSelectionPanel, panelToShow);
        SetPanel(loadingPanel, panelToShow);
    }

    private static void SetPanel(GameObject panel, GameObject panelToShow)
    {
        if (panel != null)
            panel.SetActive(panel == panelToShow);
    }

    public void GoToMapSelection()
    {
        FindNetworkObjects();

        if (runner == null || !runner.IsRunning)
        {
            Debug.LogError("NetworkRunner chưa kết nối.");
            return;
        }

        if (lobbyState == null)
        {
            Debug.LogError("Không tìm thấy LobbyState.");
            return;
        }

        if (!runner.IsServer)
        {
            Debug.LogWarning("Chỉ Host mới được chuyển sang chọn map.");
            return;
        }

        Debug.Log("Host bấm Next: chuyển sang MapSelection.");
        lobbyState.RequestOpenMapSelection();
    }

    public void GoToCharacterSelection()
{
    Debug.Log("Đã bấm Next ở màn chọn Map.");

    if (runner == null)
        runner = FindFirstObjectByType<NetworkRunner>();

    if (lobbyState == null)
        lobbyState = FindFirstObjectByType<LobbyState>();

    if (runner == null)
    {
        Debug.LogError("Không tìm thấy NetworkRunner.");
        return;
    }

    if (!runner.IsRunning)
    {
        Debug.LogError("NetworkRunner chưa chạy.");
        return;
    }

    if (lobbyState == null)
    {
        Debug.LogError("Không tìm thấy LobbyState.");
        return;
    }

    if (!runner.IsServer)
    {
        Debug.LogWarning("Chỉ Host mới được chuyển sang chọn nhân vật.");
        return;
    }

    if (!lobbyState.HasSelectedMap())
    {
        Debug.LogWarning(
            "Chưa chọn Map. SelectedMap = " +
            lobbyState.SelectedMap
        );
        return;
    }

    Debug.Log(
        "Đang chuyển sang CharacterSelection. Map = " +
        lobbyState.SelectedMap
    );

    lobbyState.RequestOpenCharacterSelection();
}

    public void BackToRoom()
    {
        FindNetworkObjects();

        if (runner != null && runner.IsServer && lobbyState != null)
            lobbyState.RequestBackToRoom();
    }

    public void BackToMapSelection()
    {
        FindNetworkObjects();

        if (runner != null && runner.IsServer && lobbyState != null)
            lobbyState.RequestBackToMapSelection();
    }

    public void StartGame()
    {
        FindNetworkObjects();

        if (runner == null || !runner.IsRunning || lobbyState == null)
        {
            Debug.LogError("Chưa kết nối hoặc không tìm thấy LobbyState.");
            return;
        }

        if (!runner.IsServer)
        {
            Debug.LogWarning("Chỉ Host mới được bắt đầu game.");
            return;
        }

        lobbyState.RequestStartGame();
    }

    public async void LeaveRoom()
    {
        if (runner != null && runner.IsRunning)
            await runner.Shutdown();

        SceneManager.LoadScene("Menu");
    }
}