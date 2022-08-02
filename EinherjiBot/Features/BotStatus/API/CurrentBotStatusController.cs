using Microsoft.AspNetCore.Mvc;
using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.BotStatus.API.Controllers
{
    [Route("api/bot/status/current")]
    [ApiController]
    public class CurrentBotStatusController : ControllerBase
    {
        private readonly IBotStatusService _service;

        public CurrentBotStatusController(IBotStatusService service)
        {
            this._service = service;
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> SetCurrentAsync(BotStatusRequest request, CancellationToken cancellationToken)
        {
            await this._service.SetCurrentAsync(request, cancellationToken).ConfigureAwait(false);
            return base.NoContent();
        }
    }
}
