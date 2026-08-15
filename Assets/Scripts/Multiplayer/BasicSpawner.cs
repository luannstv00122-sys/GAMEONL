using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network")]
    [SerializeField] private string sessionName = "TestRoom";
    [SerializeField] private int maxPlayers = 3;

    [Header("Room Code UI")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text connectionStatusText;

    [Header("Lobby")]
    [SerializeField] private NetworkPrefabRef lobbyStatePrefab;
    [SerializeField] private NetworkPrefabRef[] characterPrefabs;
    [SerializeField] private GameObject roomCodeInputPanel;

    private NetworkRunner _runner;
    private NetworkObject _lobbyStateObject;

    private readonly Dictionary<PlayerRef, int> selectedCharacters =
        new Dictionary<PlayerRef, int>();

    private async void StartGame(GameMode mode)
    {
        if (_runner != null)
        {
            Debug.LogWarning("NetworkRunner đã được tạo.");
            return;
        }

        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.AddCallbacks(this);
        _runner.ProvideInput = true;

        SceneRef currentScene =
            SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();

        if (currentScene.IsValid)
        {
            sceneInfo.AddSceneRef(
                currentScene,
                LoadSceneMode.Additive
            );
        }

        StartGameResult result = await _runner.StartGame(
            new StartGameArgs
            {
                GameMode = mode,
                SessionName = sessionName,
                PlayerCount = maxPlayers,
                Scene = sceneInfo,
                SceneManager =
                    gameObject.AddComponent<NetworkSceneManagerDefault>()
            }
        );

        if (!result.Ok)
        {
            string errorMessage =
                "Không thể kết nối phòng: " +
                result.ShutdownReason;

            Debug.LogError(errorMessage);

            if (connectionStatusText != null)
            {
                connectionStatusText.text =
                    mode == GameMode.Client
                        ? "Không tìm thấy phòng hoặc không thể kết nối."
                        : "Không thể tạo phòng.";
            }

            Destroy(_runner);
            _runner = null;
            return;
        }

        if (connectionStatusText != null)
        {
            connectionStatusText.text =
                mode == GameMode.Host
                    ? $"Đã tạo phòng: {sessionName}"
                    : $"Đã vào phòng: {sessionName}";
        }
        // Ẩn UI Enter text...
        if (roomCodeInputPanel != null)
            roomCodeInputPanel.SetActive(false);
        Debug.Log(
            mode == GameMode.Host
                ? "Đã tạo phòng: " + sessionName
                : "Đã tham gia phòng: " + sessionName
        );
    }

    public void OnHostCreateRoom()
    {
        sessionName =
            UnityEngine.Random.Range(10000, 100000).ToString();

        if (roomCodeText != null)
            roomCodeText.text = sessionName;

        if (connectionStatusText != null)
            connectionStatusText.text = "Đang tạo phòng...";

        StartGame(GameMode.Host);
    }

    public void OnClientJoinRoom()
    {
        if (roomCodeInput == null)
        {
            Debug.LogError(
                "Chưa gắn Room Code Input trong Inspector."
            );
            return;
        }

        string enteredCode = roomCodeInput.text.Trim();

        if (enteredCode.Length != 5 ||
            !int.TryParse(enteredCode, out _))
        {
            if (connectionStatusText != null)
            {
                connectionStatusText.text =
                    "Mã phòng phải gồm đúng 5 chữ số.";
            }

            return;
        }

        sessionName = enteredCode;

        if (connectionStatusText != null)
            connectionStatusText.text = "Đang tham gia phòng...";

        StartGame(GameMode.Client);
    }

    public void OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
        if (runner.IsServer && _lobbyStateObject == null)
        {
            _lobbyStateObject = runner.Spawn(
                lobbyStatePrefab,
                Vector3.zero,
                Quaternion.identity
            );
        }
    }

    public void OnPlayerLeft(
        NetworkRunner runner,
        PlayerRef player)
    {
        LobbyState lobbyState =
            FindFirstObjectByType<LobbyState>();

        if (runner.IsServer && lobbyState != null)
            lobbyState.RemovePlayer(player);
    }

    public void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData();

        if (ChatUIController.IsTyping)
        {
            input.Set(data);
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            input.Set(data);
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed ||
            keyboard.leftArrowKey.isPressed)
            horizontal -= 1f;

        if (keyboard.dKey.isPressed ||
            keyboard.rightArrowKey.isPressed)
            horizontal += 1f;

        if (keyboard.wKey.isPressed ||
            keyboard.upArrowKey.isPressed)
            vertical += 1f;

        if (keyboard.sKey.isPressed ||
            keyboard.downArrowKey.isPressed)
            vertical -= 1f;

        data.MoveInput =
            Vector2.ClampMagnitude(
                new Vector2(horizontal, vertical),
                1f
            );

        Camera localCamera = Camera.main;

        if (localCamera != null)
        {
            Vector3 forward = localCamera.transform.forward;
            Vector3 right = localCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            if (forward.sqrMagnitude > 0.001f)
                forward.Normalize();

            if (right.sqrMagnitude > 0.001f)
                right.Normalize();

            data.CameraForward = forward;
            data.CameraRight = right;
        }

        data.Buttons.Set(
            (int)PlayerInputButton.Jump,
            keyboard.spaceKey.isPressed
        );

        data.Buttons.Set(
            (int)PlayerInputButton.Sprint,
            keyboard.leftShiftKey.isPressed
        );

        data.Buttons.Set(
            (int)PlayerInputButton.Strafe,
            keyboard.tabKey.isPressed
        );

        data.Buttons.Set(
            (int)PlayerInputButton.SkillQ,
            keyboard.qKey.isPressed
        );

        data.Buttons.Set(
            (int)PlayerInputButton.SkillE,
            keyboard.eKey.isPressed
        );

        bool fire =
            Mouse.current != null &&
            Mouse.current.leftButton.isPressed;

        data.Buttons.Set(
            (int)PlayerInputButton.Fire,
            fire
        );

        input.Set(data);
    }

    public void OnInputMissing(
        NetworkRunner runner,
        PlayerRef player,
        NetworkInput input)
    {
    }

    public void OnShutdown(
        NetworkRunner runner,
        ShutdownReason shutdownReason)
    {
        _runner = null;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(
        NetworkRunner runner,
        NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    {
        request.Accept();
    }

    public void OnConnectFailed(
        NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason)
    {
        Debug.LogError(
            "Kết nối thất bại: " + reason
        );
    }

    public void OnUserSimulationMessage(
        NetworkRunner runner,
        SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(
        NetworkRunner runner,
        List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(
        NetworkRunner runner,
        Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

        if (!runner.IsServer)
            return;
            // =========================
    // QUAY LẠI LOBBY
    // =========================
    if (SceneManager.GetActiveScene().name == "Lobby")
    {
        LobbyState existingLobby =
            FindFirstObjectByType<LobbyState>();

        if (existingLobby == null)
        {
            _lobbyStateObject = runner.Spawn(
                lobbyStatePrefab,
                Vector3.zero,
                Quaternion.identity
            );

            Debug.Log("Đã spawn lại LobbyState.");
        }
        else
        {
            _lobbyStateObject = existingLobby.Object;
        }

        return;
    }

    // =========================
    // GAMEPLAY
    // =========================

        GameObject[] spawnObjects =
            GameObject.FindGameObjectsWithTag("SpawnPoint");

        Array.Sort(
            spawnObjects,
            (a, b) => string.CompareOrdinal(a.name, b.name)
        );

        if (spawnObjects.Length == 0)
        {
            Debug.Log(
                $"Scene {SceneManager.GetActiveScene().name}: " +
                "không có SpawnPoint, bỏ qua spawn gameplay."
            );
            return;
        }

        int playerIndex = 0;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (runner.GetPlayerObject(player) != null)
            {
                playerIndex++;
                continue;
            }

            if (!selectedCharacters.TryGetValue(
                    player,
                    out int characterIndex))
            {
                playerIndex++;
                continue;
            }

            if (characterIndex < 0 ||
                characterIndex >= characterPrefabs.Length)
            {
                playerIndex++;
                continue;
            }

            int spawnIndex =
                playerIndex % spawnObjects.Length;

            Transform spawnPoint =
                spawnObjects[spawnIndex].transform;

            NetworkObject playerObject =
                runner.Spawn(
                    characterPrefabs[characterIndex],
                    spawnPoint.position,
                    spawnPoint.rotation,
                    player,
                    onBeforeSpawned:
                    (spawnRunner, networkObject) =>
                    {
                        NetworkPlayerInfo info =
                            networkObject.GetComponent<NetworkPlayerInfo>();

                        if (info == null)
                        {
                            info =
                                networkObject.GetComponentInChildren<NetworkPlayerInfo>();
                        }

                        if (info != null)
                            info.CharacterIndex = characterIndex;
                    }
                );

            if (playerObject != null)
            {
                runner.SetPlayerObject(
                    player,
                    playerObject
                );
            }

            playerIndex++;
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnObjectExitAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }

    public void OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress)
    {
    }

    public void CacheLobbySelections(
        LobbyState lobbyState)
    {
        selectedCharacters.Clear();

        foreach (PlayerRef player in _runner.ActivePlayers)
        {
            int characterIndex =
                lobbyState.GetCharacterIndex(player);

            selectedCharacters[player] =
                characterIndex;
        }
    }
}