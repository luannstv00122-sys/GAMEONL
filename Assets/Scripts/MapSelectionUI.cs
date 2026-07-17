using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Map Buttons")]
    [SerializeField] private Button[] mapButtons;

    [Header("Map Images")]
    [SerializeField] private Image[] mapImages;

    [Header("Visual")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.cyan;

    [Header("Panels")]
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private GameObject characterSelectionPanel;

    [Header("Next")]
    [SerializeField] private Button nextButton;

    private NetworkRunner runner;
    private LobbyState lobbyState;

    private int selectedMapIndex = -1;

    private void Update()
    {
        if (runner == null)
            runner = FindFirstObjectByType<NetworkRunner>();

        if (lobbyState == null)
            lobbyState = FindFirstObjectByType<LobbyState>();

        if (runner == null || lobbyState == null)
            return;

        UpdateMapVisuals();

        bool isHost = runner.IsServer;

        for (int i = 0; i < mapButtons.Length; i++)
        {
            mapButtons[i].interactable = isHost;
        }

        nextButton.interactable =
            isHost && lobbyState.SelectedMap >= 0;
    }

    public void SelectMap0()
    {
        SelectMap(0);
    }

    public void SelectMap1()
    {
        SelectMap(1);
    }

    public void SelectMap2()
    {
        SelectMap(2);
    }

    public void SelectMap3()
    {
        SelectMap(3);
    }

    private void SelectMap(int index)
    {
        if (runner == null || lobbyState == null)
            return;

        if (!runner.IsServer)
            return;

        selectedMapIndex = index;
        lobbyState.RequestSelectMap(index);
    }

    public void NextToCharacterSelection()
    {
        if (runner == null || lobbyState == null)
            return;

        if (!runner.IsServer)
            return;

        if (lobbyState.SelectedMap < 0)
            return;

        mapSelectionPanel.SetActive(false);
        characterSelectionPanel.SetActive(true);
    }

    private void UpdateMapVisuals()
    {
        int currentMap = lobbyState.SelectedMap;

        for (int i = 0; i < mapImages.Length; i++)
        {
            mapImages[i].color =
                i == currentMap ? selectedColor : normalColor;
        }
    }
}