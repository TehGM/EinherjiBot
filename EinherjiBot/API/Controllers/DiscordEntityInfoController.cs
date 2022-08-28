using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TehGM.EinherjiBot.API.Controllers
{
    [Route("api/entity-info")]
    [ApiController]
    public class DiscordEntityInfoController : ControllerBase
    {
        private readonly IDiscordEntityInfoProvider _service;

        public DiscordEntityInfoController(IDiscordEntityInfoProvider service)
        {
            this._service = service;
        }

        [HttpGet("role/{id:ulong}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoleAsync(ulong id, [FromQuery(Name = "guild")] ulong[] guildIDs, CancellationToken cancellationToken)
        {
            if (guildIDs?.Any() != true)
                guildIDs = null;

            RoleInfoResponse response = await this._service.GetRoleInfoAsync(id, guildIDs, cancellationToken).ConfigureAwait(false);
            if (response == null)
                return base.NotFound();
            return base.Ok(response);
        }


        [HttpGet("bot")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBotAsync(CancellationToken cancellationToken)
        {
            UserInfoResponse response = await this._service.GetBotInfoAsync(cancellationToken).ConfigureAwait(false);
            return base.Ok(response);
        }

        [HttpGet("user/{id:ulong}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserAsync(ulong id, CancellationToken cancellationToken)
        {
            UserInfoResponse response = await this._service.GetUserInfoAsync(id, cancellationToken).ConfigureAwait(false);
            if (response == null)
                return base.NotFound();
            return base.Ok(response);
        }

        [HttpGet("user")]
        [Authorize]
        public Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
        {
            IDiscordAuthContext context = base.HttpContext.Features.Get<IDiscordAuthContext>();
            if (context == null || !context.IsLoggedIn() || context.IsBanned)
                return Task.FromResult((IActionResult)base.Unauthorized());
            return this.GetUserAsync(context.ID, cancellationToken);
        }

        [HttpGet("guilds")]
        [Authorize]
        public async Task<IActionResult> GetGuildsAsync([FromQuery(Name = "guild")] ulong[] guildIDs, CancellationToken cancellationToken)
        {
            if (guildIDs?.Any() != true)
                guildIDs = null;

            IEnumerable<GuildInfoResponse> responses = await this._service.GetGuildInfosAsync(guildIDs, cancellationToken).ConfigureAwait(false);
            if (guildIDs != null && !responses.Any())
                return base.NotFound(responses);
            return base.Ok(responses);
        }

        [HttpGet("guild/{id:ulong}")]
        [Authorize]
        public async Task<IActionResult> GetGuildAsync(ulong id, CancellationToken cancellationToken)
        {
            IEnumerable<GuildInfoResponse> responses = await this._service.GetGuildInfosAsync(new[] { id }, cancellationToken).ConfigureAwait(false);
            if (!responses.Any())
                return base.NotFound();
            return base.Ok(responses);
        }

        [HttpGet("guild/{guildID:ulong}/user/{userID:ulong}")]
        [Authorize]
        public async Task<IActionResult> GetGuildUserAsync(ulong guildID, ulong userID, CancellationToken cancellationToken)
        {
            GuildUserInfoResponse response = await this._service.GetGuildUserInfoAsync(userID, guildID, cancellationToken).ConfigureAwait(false);
            if (response == null)
                return base.NotFound();
            return base.Ok(response);
        }

        [HttpGet("guild/{guildID:ulong}/channel/{channelID:ulong}")]
        [Authorize]
        public Task<IActionResult> GetGuildChannelAsync(ulong guildID, ulong channelID, CancellationToken cancellationToken)
            => this.GetChannelAsync(channelID, new[] { guildID }, cancellationToken);

        [HttpGet("channel/{id:ulong}")]
        [Authorize]
        public async Task<IActionResult> GetChannelAsync(ulong id, [FromQuery(Name = "guild")] ulong[] guildIDs, CancellationToken cancellationToken)
        {
            if (guildIDs?.Any() != true)
                guildIDs = null;

            ChannelInfoResponse response = await this._service.GetChannelInfoAsync(id, guildIDs, cancellationToken).ConfigureAwait(false);
            if (response == null)
                return base.NotFound();
            return base.Ok(response);
        }
    }
}
