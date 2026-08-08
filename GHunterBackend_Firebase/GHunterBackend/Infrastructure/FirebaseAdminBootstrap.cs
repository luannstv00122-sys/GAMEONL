using FirebaseAdmin;
using GHunterBackend.Options;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace GHunterBackend.Infrastructure;
public sealed class FirebaseAdminBootstrap
{
    readonly FirebaseOptions _o; readonly IHostEnvironment _env; readonly ILogger<FirebaseAdminBootstrap> _log;
    bool _ready;
    public GoogleCredential DatabaseCredential { get; private set; } = null!;
    public FirebaseAdminBootstrap(IOptions<FirebaseOptions> o,IHostEnvironment env,ILogger<FirebaseAdminBootstrap> log){_o=o.Value;_env=env;_log=log;}
    public void Initialize()
    {
        if(_ready) return;
        var path=_o.ServiceAccountPath;
        if(!Path.IsPathRooted(path)) path=Path.Combine(_env.ContentRootPath,path);
        path=Path.GetFullPath(path);
        if(!File.Exists(path)) throw new FileNotFoundException("Thiếu secrets/firebase-admin.json. Xem README.md",path);
        var cred=GoogleCredential.FromFile(path);
        try { _ = FirebaseApp.DefaultInstance; }
        catch(InvalidOperationException) { FirebaseApp.Create(new AppOptions{Credential=cred,ProjectId=_o.ProjectId}); }
        DatabaseCredential=cred.CreateScoped(new[]{"https://www.googleapis.com/auth/firebase.database","https://www.googleapis.com/auth/userinfo.email"});
        _ready=true; _log.LogInformation("Firebase Admin READY. Project={Project}",_o.ProjectId);
    }
}
