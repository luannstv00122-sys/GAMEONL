using GHunterBackend.Infrastructure;
using GHunterBackend.Options;
using GHunterBackend.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.Configure<FirebaseOptions>(builder.Configuration.GetSection(FirebaseOptions.SectionName));
builder.Services.Configure<BackupOptions>(builder.Configuration.GetSection(BackupOptions.SectionName));
builder.Services.AddHttpClient("FirebaseRtdb", c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddSingleton<FirebaseAdminBootstrap>();
builder.Services.AddSingleton<FirebaseRealtimeDbService>();
builder.Services.AddSingleton<DatabaseBackupService>();
builder.Services.AddHostedService<DatabaseBackupWorker>();

var app = builder.Build();
app.Services.GetRequiredService<FirebaseAdminBootstrap>().Initialize();
app.UseMiddleware<FirebaseAuthMiddleware>();
app.MapControllers();
app.Run();
