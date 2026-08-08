using GHunterBackend.Infrastructure;
using GHunterBackend.Models;
using GHunterBackend.Services;
using Microsoft.AspNetCore.Mvc;
namespace GHunterBackend.Controllers;
[ApiController][Route("api/player")]
public sealed class PlayerController:ControllerBase
{
    readonly FirebaseRealtimeDbService _db; public PlayerController(FirebaseRealtimeDbService db){_db=db;}
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var uid=Uid(); var p=await _db.GetAsync<PlayerSecureData>($"securePlayers/{uid}",ct);
        if(p==null){var now=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();p=new PlayerSecureData{Uid=uid,Level=1,CreatedAt=now,UpdatedAt=now};await _db.PutAsync($"securePlayers/{uid}",p,ct);} return Ok(p);
    }
    [HttpPost("character")]
    public async Task<IActionResult> Character([FromBody]SelectCharacterRequest r,CancellationToken ct)
    {
        if(r.CharacterId<0||r.CharacterId>99)return BadRequest(new{error="CharacterId không hợp lệ"});var uid=Uid();await _db.PatchAsync($"securePlayers/{uid}",new{uid,selectedCharacter=r.CharacterId,updatedAt=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()},ct);return Ok(new{ok=true,selectedCharacter=r.CharacterId});
    }
    string Uid()=>HttpContext.Items[FirebaseAuthMiddleware.UidItemKey] as string??throw new UnauthorizedAccessException();
}
