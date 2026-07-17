using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network")]
    [SerializeField] private string sessionName = "TestRoom";
    [SerializeField] private int maxPlayers = 3;

    [Header("Lobby")]
    [SerializeField] private NetworkPrefabRef lobbyStatePrefab;
    [SerializeField]
    private NetworkPrefabRef[] characterPrefabs;

    private NetworkRunner _runner;
    private NetworkObject _lobbyStateObject;
    private readonly Dictionary<PlayerRef, int> selectedCharacters =
    new Dictionary<PlayerRef, int>();

    private async void StartGame(GameMode mode)
    {
        // Tránh tạo nhiều NetworkRunner khi nhấn nút nhiều lần.
        if (_runner != null)
        {
            Debug.LogWarning("NetworkRunner đã được tạo.");
            return;
        }

        _runner = gameObject.AddComponent<NetworkRunner>();

        // Đăng ký callback cho script này.
        _runner.AddCallbacks(this);

        // Cho phép gửi input mạng.
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

                // Phòng tối đa 3 người.
                PlayerCount = maxPlayers,

                Scene = sceneInfo,

                SceneManager =
                    gameObject.AddComponent<NetworkSceneManagerDefault>()
            }
        );

        if (!result.Ok)
        {
            Debug.LogError(
                "Không thể kết nối phòng: " +
                result.ShutdownReason
            );

            Destroy(_runner);
            _runner = null;
            return;
        }

        Debug.Log(
            mode == GameMode.Host
                ? "Đã tạo phòng: " + sessionName
                : "Đã tham gia phòng: " + sessionName
        );
    }

    // Nút Host tạo phòng.
    public void OnHostCreateRoom()
    {
        StartGame(GameMode.Host);
    }

    // Nút Client tham gia phòng.
    public void OnClientJoinRoom()
    {
        StartGame(GameMode.Client);
    }

    public void OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
        Debug.Log(
            "Player " + player.PlayerId + " đã vào phòng."
        );

        // Host tạo LobbyState dùng chung cho cả phòng.
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
        Debug.Log(
            "Player " + player.PlayerId + " đã rời phòng."
        );

        LobbyState lobbyState =
            FindFirstObjectByType<LobbyState>();

        if (runner.IsServer && lobbyState != null)
        {
            lobbyState.RemovePlayer(player);
        }
    }

    public void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
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

        Debug.Log(
            "Fusion Shutdown: " + shutdownReason
        );
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Đã kết nối đến server.");
    }

    public void OnDisconnectedFromServer(
        NetworkRunner runner,
        NetDisconnectReason reason)
    {
        Debug.LogWarning(
            "Đã mất kết nối: " + reason
        );
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

    GameObject[] spawnObjects =
        GameObject.FindGameObjectsWithTag("SpawnPoint");

    if (spawnObjects.Length == 0)
    {
        Debug.LogError("Không tìm thấy SpawnPoint.");
        return;
    }

    foreach (PlayerRef player in runner.ActivePlayers)
    {
        if (runner.GetPlayerObject(player) != null)
            continue;

        if (!selectedCharacters.TryGetValue(
                player,
                out int characterIndex))
        {
            Debug.LogError(
                $"Không có dữ liệu Character của Player {player.PlayerId}"
            );

            continue;
        }

        if (characterIndex < 0 ||
            characterIndex >= characterPrefabs.Length)
        {
            Debug.LogError(
                $"Character index không hợp lệ: {characterIndex}"
            );

            continue;
        }

        int spawnIndex =
            player.PlayerId % spawnObjects.Length;

        Transform spawnPoint =
            spawnObjects[spawnIndex].transform;

        NetworkObject playerObject =
    runner.Spawn(
        characterPrefabs[characterIndex],
        spawnPoint.position,
        spawnPoint.rotation,
        player
    );

if (playerObject == null)
{
    Debug.LogError("Spawn FAILED");
    return;
}

Debug.Log("Spawn OK: " + playerObject.name);

runner.SetPlayerObject(player, playerObject);
Debug.Log(
    $"Spawned object: {playerObject.name}, " +
    $"Active={playerObject.gameObject.activeInHierarchy}, " +
    $"Scene={playerObject.gameObject.scene.name}, " +
    $"Position={playerObject.transform.position}"
);

Debug.Log("PlayerObject = " + runner.GetPlayerObject(player));
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
    public void CacheLobbySelections(LobbyState lobbyState)
{
    selectedCharacters.Clear();

    foreach (PlayerRef player in _runner.ActivePlayers)
    {
        int characterIndex =
            lobbyState.GetCharacterIndex(player);

        selectedCharacters[player] = characterIndex;

        Debug.Log(
            $"Lưu Player {player.PlayerId} - Character {characterIndex}"
        );
    }
}
}