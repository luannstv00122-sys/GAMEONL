using GHunterBackend.Options;
using Microsoft.Extensions.Options;
namespace GHunterBackend.Services;
public sealed class DatabaseBackupWorker:BackgroundService
{
    readonly DatabaseBackupService _b; readonly BackupOptions _o; public DatabaseBackupWorker(DatabaseBackupService b,IOptions<BackupOptions> o){_b=b;_o=o.Value;}
    protected override async Task ExecuteAsync(CancellationToken ct){if(!_o.Enabled)return;if(_o.BackupOnStartup)try{await _b.BackupAsync("startup",ct);}catch{}using var timer=new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1,_o.IntervalMinutes)));while(await timer.WaitForNextTickAsync(ct))try{await _b.BackupAsync("auto",ct);}catch{}}
}
