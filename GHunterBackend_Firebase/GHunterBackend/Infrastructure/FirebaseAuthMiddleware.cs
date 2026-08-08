using FirebaseAdmin.Auth;
namespace GHunterBackend.Infrastructure;
public sealed class FirebaseAuthMiddleware
{
    public const string UidItemKey="FirebaseUid"; readonly RequestDelegate _next;
    public FirebaseAuthMiddleware(RequestDelegate next){_next=next;}
    public async Task InvokeAsync(HttpContext ctx)
    {
        if(!ctx.Request.Path.StartsWithSegments("/api/player")){await _next(ctx);return;}
        var h=ctx.Request.Headers.Authorization.ToString();
        if(!h.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase)){ctx.Response.StatusCode=401;await ctx.Response.WriteAsJsonAsync(new{error="Thiếu Firebase ID token"});return;}
        try{var decoded=await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(h[7..].Trim());ctx.Items[UidItemKey]=decoded.Uid;await _next(ctx);}
        catch{ctx.Response.StatusCode=401;await ctx.Response.WriteAsJsonAsync(new{error="Firebase token không hợp lệ hoặc đã hết hạn"});}
    }
}
