using Microsoft.AspNetCore.Mvc;

namespace TehGM.EinherjiBot.Settings.Controllers
{
    [Route("api/guild/{guildID:ulong}/settings")]
    [ApiController]
    public class GuildSettingsController : ControllerBase
    {
        private readonly IGuildSettingsHandler _handler;

        public GuildSettingsController(IGuildSettingsHandler service)
        {
            this._handler = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(ulong guildID, CancellationToken cancellationToken)
        {
            GuildSettingsResponse result = await this._handler.GetAsync(guildID, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return base.NotFound();
            return base.Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(ulong guildID, GuildSettingsRequest request, CancellationToken cancellationToken)
        {
            GuildSettingsResponse result = await this._handler.UpdateAsync(guildID, request, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return base.NotFound();
            return base.Ok(result);
        }
    }
}
