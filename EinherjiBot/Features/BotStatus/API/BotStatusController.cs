using Microsoft.AspNetCore.Mvc;
using TehGM.EinherjiBot.Security.Authorization.Policies;

namespace TehGM.EinherjiBot.BotStatus.API.Controllers
{
    [Route("api/bot/status")]
    [ApiController]
    public class BotStatusController : ControllerBase
    {
        private readonly IBotStatusService _service;

        public BotStatusController(IBotStatusService service)
        {
            this._service = service;
        }

        [HttpGet]
        [AuthorizeAdmin]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        {
            IEnumerable<BotStatusResponse> results = await this._service.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return base.Ok(results);
        }

        [HttpGet("id:guid")]
        [ActionName(nameof(GetAsync))]
        [AuthorizeAdmin]
        public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            BotStatusResponse result = await this._service.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return base.NotFound();
            return base.Ok(result);
        }

        [HttpPost]
        [AuthorizeAdmin]
        public async Task<IActionResult> CreateAsync(BotStatusRequest request, CancellationToken cancellationToken)
        {
            BotStatusResponse result = await this._service.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            return base.CreatedAtAction(nameof(GetAsync), new { id = result.ID }, result);
        }

        [HttpPut("id:guid")]
        [AuthorizeAdmin]
        public async Task<IActionResult> UpdateAsync(Guid id, BotStatusRequest request, CancellationToken cancellationToken)
        {
            BotStatusResponse result = await this._service.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return base.NotFound();
            return base.Ok(result);
        }

        [HttpDelete("id:guid")]
        [AuthorizeAdmin]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            await this._service.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            return base.NoContent();
        }
    }
}
