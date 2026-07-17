using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum LobbyStage : byte
{
    Room = 0,
    MapSelection = 1,
    CharacterSelection = 2,
    Loading = 3
}

public class LobbyState : NetworkBehaviour
{
    private const int CharacterCount = 6;

    [Header("Scene Settings")]
    [Tooltip("Build Index của Map đầu tiên trong Build Profiles.")]
    [SerializeField] private int firstMapBuildIndex = 2;

    [Tooltip("Tổng số map có thể chọn.")]
    [SerializeField] private int totalMaps = 4;

    // -1 nghĩa là Host chưa chọn map.
    [Networked] public int SelectedMap { get; set; }

    [Networked] public NetworkBool GameStarted { get; set; }

    [Networked] public LobbyStage CurrentStage { get; set; }

    // =========================
    // CHARACTER OWNERS
    // =========================

    [Networked] public PlayerRef Character0Owner { get; set; }
    [Networked] public PlayerRef Character1Owner { get; set; }
    [Networked] public PlayerRef Character2Owner { get; set; }
    [Networked] public PlayerRef Character3Owner { get; set; }
    [Networked] public PlayerRef Character4Owner { get; set; }
    [Networked] public PlayerRef Character5Owner { get; set; }

    // =========================
    // READY STATES
    // =========================

    [Networked] public NetworkBool Character0Ready { get; set; }
    [Networked] public NetworkBool Character1Ready { get; set; }
    [Networked] public NetworkBool Character2Ready { get; set; }
    [Networked] public NetworkBool Character3Ready { get; set; }
    [Networked] public NetworkBool Character4Ready { get; set; }
    [Networked] public NetworkBool Character5Ready { get; set; }
    private int _pendingMapBuildIndex = -1;
    private void LoadSelectedMap()
{
    if (_pendingMapBuildIndex < 0)
        return;

    Runner.LoadScene(
        SceneRef.FromIndex(_pendingMapBuildIndex),
        LoadSceneMode.Single
    );
}
    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
            return;

        SelectedMap = -1;
        GameStarted = false;
        CurrentStage = LobbyStage.Room;

        ClearAllCharacters();
    }

    // =========================
    // CHỌN 1 TRONG 4 MAP
    // =========================

    public void RequestSelectMap(int mapIndex)
    {
        if (Object.HasStateAuthority)
        {
            ApplySelectedMap(mapIndex);
            return;
        }

        RPC_SelectMap(mapIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SelectMap(int mapIndex, RpcInfo info = default)
    {
        ApplySelectedMap(mapIndex);
    }

    private void ApplySelectedMap(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= totalMaps)
        {
            Debug.LogWarning($"Map index không hợp lệ: {mapIndex}");
            return;
        }

        SelectedMap = mapIndex;
        ResetAllReady();

        Debug.Log($"Đã chọn map index: {mapIndex}");
    }

    public bool HasSelectedMap()
    {
        return SelectedMap >= 0 && SelectedMap < totalMaps;
    }

    // =========================
    // CHỌN CHARACTER
    // =========================

    public void RequestCharacter(int characterIndex)
    {
        if (Object.HasStateAuthority)
        {
            TrySelectCharacter(characterIndex, Runner.LocalPlayer);
            return;
        }

        RPC_SelectCharacter(characterIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SelectCharacter(
        int characterIndex,
        RpcInfo info = default)
    {
        TrySelectCharacter(characterIndex, info.Source);
    }

    private void TrySelectCharacter(
        int characterIndex,
        PlayerRef player)
    {
        if (!IsValidCharacterIndex(characterIndex))
        {
            Debug.LogWarning($"Character index không hợp lệ: {characterIndex}");
            return;
        }

        if (!HasSelectedMap())
        {
            Debug.LogWarning("Host chưa chọn map.");
            return;
        }

        if (player.IsNone)
        {
            Debug.LogError("Không xác định được PlayerRef khi chọn nhân vật.");
            return;
        }

        if (IsPlayerReady(player))
        {
            Debug.LogWarning("Hãy Cancel Ready trước khi đổi nhân vật.");
            return;
        }

        PlayerRef currentOwner = GetCharacterOwner(characterIndex);

        if (!currentOwner.IsNone && currentOwner != player)
        {
            Debug.LogWarning("Nhân vật này đã được người chơi khác chọn.");
            return;
        }

        ReleaseCharacter(player);
        SetCharacterOwner(characterIndex, player);

        Debug.Log(
            $"Đã xác nhận Player {player.PlayerId} chọn Character {characterIndex}"
        );
    }

    public void RequestCancelCharacter()
    {
        if (Object.HasStateAuthority)
        {
            TryCancelCharacter(Runner.LocalPlayer);
            return;
        }

        RPC_CancelCharacter();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_CancelCharacter(RpcInfo info = default)
    {
        TryCancelCharacter(info.Source);
    }

    private void TryCancelCharacter(PlayerRef player)
    {
        if (player.IsNone)
        {
            Debug.LogError("Không xác định được PlayerRef khi hủy nhân vật.");
            return;
        }

        if (IsPlayerReady(player))
        {
            Debug.LogWarning("Hãy Cancel Ready trước khi hủy nhân vật.");
            return;
        }

        ReleaseCharacter(player);
        Debug.Log($"Player {player.PlayerId} đã hủy Character.");
    }

    public int GetCharacterIndex(PlayerRef player)
    {
        if (player.IsNone)
            return -1;

        if (Character0Owner == player) return 0;
        if (Character1Owner == player) return 1;
        if (Character2Owner == player) return 2;
        if (Character3Owner == player) return 3;
        if (Character4Owner == player) return 4;
        if (Character5Owner == player) return 5;

        return -1;
    }

    public PlayerRef GetCharacterOwner(int index)
    {
        return index switch
        {
            0 => Character0Owner,
            1 => Character1Owner,
            2 => Character2Owner,
            3 => Character3Owner,
            4 => Character4Owner,
            5 => Character5Owner,
            _ => PlayerRef.None
        };
    }

    public bool IsCharacterAvailable(int characterIndex, PlayerRef player)
    {
        if (!IsValidCharacterIndex(characterIndex))
            return false;

        PlayerRef owner = GetCharacterOwner(characterIndex);
        return owner.IsNone || owner == player;
    }

    private void SetCharacterOwner(int characterIndex, PlayerRef player)
    {
        switch (characterIndex)
        {
            case 0:
                Character0Owner = player;
                Character0Ready = false;
                break;

            case 1:
                Character1Owner = player;
                Character1Ready = false;
                break;

            case 2:
                Character2Owner = player;
                Character2Ready = false;
                break;

            case 3:
                Character3Owner = player;
                Character3Ready = false;
                break;

            case 4:
                Character4Owner = player;
                Character4Ready = false;
                break;

            case 5:
                Character5Owner = player;
                Character5Ready = false;
                break;
        }
    }

    private void ReleaseCharacter(PlayerRef player)
    {
        if (Character0Owner == player)
        {
            Character0Owner = PlayerRef.None;
            Character0Ready = false;
        }

        if (Character1Owner == player)
        {
            Character1Owner = PlayerRef.None;
            Character1Ready = false;
        }

        if (Character2Owner == player)
        {
            Character2Owner = PlayerRef.None;
            Character2Ready = false;
        }

        if (Character3Owner == player)
        {
            Character3Owner = PlayerRef.None;
            Character3Ready = false;
        }

        if (Character4Owner == player)
        {
            Character4Owner = PlayerRef.None;
            Character4Ready = false;
        }

        if (Character5Owner == player)
        {
            Character5Owner = PlayerRef.None;
            Character5Ready = false;
        }
    }

    // =========================
    // READY
    // =========================

    public void RequestToggleReady()
    {
        if (Object.HasStateAuthority)
        {
            TryToggleReady(Runner.LocalPlayer);
            return;
        }

        RPC_ToggleReady();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ToggleReady(RpcInfo info = default)
    {
        TryToggleReady(info.Source);
    }

    private void TryToggleReady(PlayerRef player)
    {
        if (player.IsNone)
        {
            Debug.LogError("Không xác định được PlayerRef khi Ready.");
            return;
        }

        int characterIndex = GetCharacterIndex(player);

        if (!HasSelectedMap())
        {
            Debug.LogWarning("Phải chọn map trước khi Ready.");
            return;
        }

        if (characterIndex < 0)
        {
            Debug.LogWarning("Phải chọn nhân vật trước khi Ready.");
            return;
        }

        NetworkBool newValue = !GetCharacterReady(characterIndex);
        SetCharacterReady(characterIndex, newValue);

        Debug.Log($"Player {player.PlayerId} Ready = {newValue}");
    }

    public bool IsPlayerReady(PlayerRef player)
    {
        int characterIndex = GetCharacterIndex(player);

        return characterIndex >= 0 &&
               GetCharacterReady(characterIndex);
    }

    private NetworkBool GetCharacterReady(int characterIndex)
    {
        return characterIndex switch
        {
            0 => Character0Ready,
            1 => Character1Ready,
            2 => Character2Ready,
            3 => Character3Ready,
            4 => Character4Ready,
            5 => Character5Ready,
            _ => false
        };
    }

    private void SetCharacterReady(
        int characterIndex,
        NetworkBool value)
    {
        switch (characterIndex)
        {
            case 0:
                Character0Ready = value;
                break;

            case 1:
                Character1Ready = value;
                break;

            case 2:
                Character2Ready = value;
                break;

            case 3:
                Character3Ready = value;
                break;

            case 4:
                Character4Ready = value;
                break;

            case 5:
                Character5Ready = value;
                break;
        }
    }

    private void ResetAllReady()
    {
        Character0Ready = false;
        Character1Ready = false;
        Character2Ready = false;
        Character3Ready = false;
        Character4Ready = false;
        Character5Ready = false;
    }


    // =========================
    // CHUYỂN GIAI ĐOẠN LOBBY
    // =========================

    public void RequestOpenMapSelection()
    {
        if (Object.HasStateAuthority)
        {
            SetLobbyStage(LobbyStage.MapSelection);
            return;
        }

        RPC_OpenMapSelection();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_OpenMapSelection(RpcInfo info = default)
    {
        SetLobbyStage(LobbyStage.MapSelection);
    }

    public void RequestOpenCharacterSelection()
    {
        if (Object.HasStateAuthority)
        {
            TryOpenCharacterSelection();
            return;
        }

        RPC_OpenCharacterSelection();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_OpenCharacterSelection(RpcInfo info = default)
    {
        TryOpenCharacterSelection();
    }

    private void TryOpenCharacterSelection()
    {
        if (!HasSelectedMap())
        {
            Debug.LogWarning("Host phải chọn một map trước.");
            return;
        }

        SetLobbyStage(LobbyStage.CharacterSelection);
    }

    public void RequestBackToRoom()
    {
        if (Object.HasStateAuthority)
        {
            SetLobbyStage(LobbyStage.Room);
            return;
        }

        RPC_BackToRoom();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_BackToRoom(RpcInfo info = default)
    {
        SetLobbyStage(LobbyStage.Room);
    }

    public void RequestBackToMapSelection()
    {
        if (Object.HasStateAuthority)
        {
            SetLobbyStage(LobbyStage.MapSelection);
            return;
        }

        RPC_BackToMapSelection();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_BackToMapSelection(RpcInfo info = default)
    {
        SetLobbyStage(LobbyStage.MapSelection);
    }

    private void SetLobbyStage(LobbyStage stage)
    {
        CurrentStage = stage;
        Debug.Log($"Lobby chuyển sang: {stage}");
    }

    // =========================
    // START GAME
    // =========================

    public void RequestStartGame()
    {
        if (Object.HasStateAuthority)
        {
            TryStartGame();
            return;
        }

        RPC_StartGame();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_StartGame(RpcInfo info = default)
    {
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (GameStarted)
            return;

        if (!CanStartGame())
        {
            Debug.LogWarning(
                "Chưa thể bắt đầu: cần chọn map, mọi người chọn nhân vật và Ready."
            );
            return;
        }
        BasicSpawner spawner =
    FindFirstObjectByType<BasicSpawner>();

if (spawner != null)
{
    spawner.CacheLobbySelections(this);
}
else
{
    Debug.LogError("Không tìm thấy BasicSpawner.");
    return;
}

        int mapBuildIndex = firstMapBuildIndex + SelectedMap;

        GameStarted = true;
SetLobbyStage(LobbyStage.Loading);

_pendingMapBuildIndex = mapBuildIndex;
Invoke(nameof(LoadSelectedMap), 5f);
    }

    public bool CanStartGame()
    {
        if (!HasSelectedMap())
            return false;

        int playerCount = 0;

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            playerCount++;

            if (GetCharacterIndex(player) < 0)
                return false;

            if (!IsPlayerReady(player))
                return false;
        }

        return playerCount > 0;
    }

    // =========================
    // PLAYER RỜI PHÒNG
    // =========================

    public void RemovePlayer(PlayerRef player)
    {
        if (!Object.HasStateAuthority)
            return;

        ReleaseCharacter(player);
    }

    // =========================
    // HELPERS
    // =========================

    private static bool IsValidCharacterIndex(int index)
    {
        return index >= 0 && index < CharacterCount;
    }

    private void ClearAllCharacters()
    {
        Character0Owner = PlayerRef.None;
        Character1Owner = PlayerRef.None;
        Character2Owner = PlayerRef.None;
        Character3Owner = PlayerRef.None;
        Character4Owner = PlayerRef.None;
        Character5Owner = PlayerRef.None;

        ResetAllReady();
    }
    
}