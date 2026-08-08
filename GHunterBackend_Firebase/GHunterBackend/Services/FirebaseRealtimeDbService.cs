using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GHunterBackend.Infrastructure;
using GHunterBackend.Options;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace GHunterBackend.Services;
public sealed class FirebaseRealtimeDbService
{
    readonly IHttpClientFactory _factory; readonly FirebaseAdminBootstrap _fb; readonly string _url;
    static readonly JsonSerializerOptions J=new(){PropertyNamingPolicy=JsonNamingPolicy.CamelCase,PropertyNameCaseInsensitive=true};
    public FirebaseRealtimeDbService(IHttpClientFactory f,FirebaseAdminBootstrap fb,IOptions<FirebaseOptions> o){_factory=f;_fb=fb;_url=o.Value.DatabaseUrl.TrimEnd('/');}
    public async Task<T?> GetAsync<T>(string path,CancellationToken ct=default)
    {
        using var req=await Req(HttpMethod.Get,path,null,ct); using var res=await _factory.CreateClient("FirebaseRtdb").SendAsync(req,ct); var body=await res.Content.ReadAsStringAsync(ct); Ensure(res,body); if(body=="null"||string.IsNullOrWhiteSpace(body)) return default; return JsonSerializer.Deserialize<T>(body,J);
    }
    public Task PutAsync<T>(string path,T value,CancellationToken ct=default)=>Write(HttpMethod.Put,path,JsonSerializer.Serialize(value,J),ct);
    public Task PatchAsync<T>(string path,T value,CancellationToken ct=default)=>Write(HttpMethod.Patch,path,JsonSerializer.Serialize(value,J),ct);
    public async Task<string> GetRootJsonAsync(CancellationToken ct=default){using var req=await Req(HttpMethod.Get,"",null,ct);using var res=await _factory.CreateClient("FirebaseRtdb").SendAsync(req,ct);var b=await res.Content.ReadAsStringAsync(ct);Ensure(res,b);return b;}
    async Task Write(HttpMethod m,string p,string j,CancellationToken ct){using var req=await Req(m,p,j,ct);using var res=await _factory.CreateClient("FirebaseRtdb").SendAsync(req,ct);var b=await res.Content.ReadAsStringAsync(ct);Ensure(res,b);}
    async Task<HttpRequestMessage> Req(HttpMethod m,string path,string? json,CancellationToken ct)
    {
        var token=await ((ITokenAccess)_fb.DatabaseCredential.UnderlyingCredential).GetAccessTokenForRequestAsync(cancellationToken:ct);
        var req=new HttpRequestMessage(m,Build(path));req.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);if(json!=null)req.Content=new StringContent(json,Encoding.UTF8,"application/json");return req;
    }
    string Build(string path){path=(path??"").Trim('/');if(path.Length==0)return _url+"/.json";var e=string.Join("/",path.Split('/').Select(Uri.EscapeDataString));return $"{_url}/{e}.json";}
    static void Ensure(HttpResponseMessage r,string b){if(!r.IsSuccessStatusCode)throw new HttpRequestException($"Firebase RTDB HTTP {(int)r.StatusCode}: {b}");}
}
