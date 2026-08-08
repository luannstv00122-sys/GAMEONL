using GHunterBackend.Options;
using Microsoft.Extensions.Options;
namespace GHunterBackend.Services;
public sealed class DatabaseBackupService
{
    static readonly SemaphoreSlim Gate=new(1,1); readonly FirebaseRealtimeDbService _db; readonly BackupOptions _o; readonly IHostEnvironment _env;
    public DatabaseBackupService(FirebaseRealtimeDbService db,IOptions<BackupOptions> o,IHostEnvironment env){_db=db;_o=o.Value;_env=env;}
    public async Task<string> BackupAsync(string reason,CancellationToken ct=default)
    {
        await Gate.WaitAsync(ct);try{var json=await _db.GetRootJsonAsync(ct);var dir=Path.IsPathRooted(_o.Directory)?_o.Directory:Path.Combine(_env.ContentRootPath,_o.Directory);Directory.CreateDirectory(dir);var file=Path.Combine(dir,$"firebase_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{reason}.json");await File.WriteAllTextAsync(file,json,ct);foreach(var old in Directory.GetFiles(dir,"*.json").Select(x=>new FileInfo(x)).OrderByDescending(x=>x.CreationTimeUtc).Skip(Math.Max(1,_o.KeepLatest)))old.Delete();return file;}finally{Gate.Release();}
    }
}
