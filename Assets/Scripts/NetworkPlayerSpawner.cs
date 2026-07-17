using Fusion;
using UnityEngine;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] private NetworkPrefabRef[] characterPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private NetworkRunner _runner;
    private LobbyState _lobbyState;
    private bool _spawned;

    private void Update()
    {
        if (_spawned)
            return;

        if (_runner == null)
        {
            _runner = FindFirstObjectByType<NetworkRunner>();
        }

        if (_lobbyState == null)
        {
            _lobbyState = FindFirstObjectByType<LobbyState>();
        }

        if (_runner == null || _lobbyState == null)
            return;

        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        int characterIndex =
            _lobbyState.GetCharacterIndex(
                _runner.LocalPlayer
            );

        if (characterIndex < 0)
        {
            Debug.LogError(
                "Player chưa chọn Character."
            );

            return;
        }

        if (characterIndex >= characterPrefabs.Length)
        {
            Debug.LogError(
                "Character Prefab không tồn tại."
            );

            return;
        }

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogError(
                "Chưa thêm Spawn Point."
            );

            return;
        }

        int spawnIndex =
            _runner.LocalPlayer.PlayerId %
            spawnPoints.Length;

        Transform spawnPoint =
            spawnPoints[spawnIndex];

        _runner.Spawn(
            characterPrefabs[characterIndex],
            spawnPoint.position,
            spawnPoint.rotation,
            _runner.LocalPlayer
        );

        _spawned = true;
    }
}