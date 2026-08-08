namespace GHunterBackend.Options;
public sealed class BackupOptions
{
    public const string SectionName = "Backup";
    public bool Enabled { get; set; } = true;
    public bool BackupOnStartup { get; set; } = true;
    public int IntervalMinutes { get; set; } = 30;
    public int KeepLatest { get; set; } = 20;
    public string Directory { get; set; } = "Backups";
    public string AdminKey { get; set; } = "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET";
}
