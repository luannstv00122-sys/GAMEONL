namespace GHunterBackend.Models;
public sealed class PlayerSecureData
{
    public string Uid { get; set; }="";
    public int SelectedCharacter { get; set; }
    public int Level { get; set; }=1;
    public long Xp { get; set; }
    public long Coins { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
}
