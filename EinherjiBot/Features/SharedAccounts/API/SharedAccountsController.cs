using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TehGM.EinherjiBot.SharedAccounts.Controllers
{
    [Route("api/shared-accounts")]
    [ApiController]
    public class SharedAccountsController : ControllerBase
    {
        private readonly ISharedAccountHandler _handler;

        public SharedAccountsController(ISharedAccountHandler service)
        {
            this._handler = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        {
            IEnumerable<SharedAccountResponse> results = await this._handler.GetAllAsync(null, skipAudit: false, cancellationToken).ConfigureAwait(false);
            return base.Ok(results);
        }

        [HttpGet("{id:guid}")]
        [ActionName(nameof(GetAsync))]
        public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            SharedAccountResponse result = await this._handler.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return base.NotFound();
            return base.Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(SharedAccountRequest request, CancellationToken cancellationToken)
        {
            SharedAccountResponse result = await this._handler.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            return base.CreatedAtAction(nameof(GetAsync), new { id = result.ID }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, SharedAccountRequest request, CancellationToken cancellationToken)
        {
            SharedAccountResponse result = await this._handler.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return base.NotFound();
            return base.Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            await this._handler.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            return base.NoContent();
        }

        [HttpGet("images")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImages(CancellationToken cancellationToken)
        {
            IDictionary<SharedAccountType, string> images = await this._handler.GetImagesAsync(cancellationToken).ConfigureAwait(false);
            return base.Ok(images);
        }
    }
}
