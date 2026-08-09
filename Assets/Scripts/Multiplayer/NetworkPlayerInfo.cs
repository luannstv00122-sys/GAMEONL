using Fusion;

public class NetworkPlayerInfo : NetworkBehaviour
{
    [Networked]
    public int CharacterIndex { get; set; } = -1;
}