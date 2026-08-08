namespace GHunterBackend.Options;
public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";
    public string ProjectId { get; set; } = "";
    public string DatabaseUrl { get; set; } = "";
    public string ServiceAccountPath { get; set; } = "secrets/firebase-admin.json";
}
