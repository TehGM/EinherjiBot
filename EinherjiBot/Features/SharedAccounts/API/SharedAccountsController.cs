using Microsoft.AspNetCore.Mvc;

namespace TehGM.EinherjiBot.SharedAccounts.API.Controllers
{
    [Route("api/shared-accounts")]
    [ApiController]
    public class SharedAccountsController : ControllerBase
    {
        private readonly ISharedAccountsService _service;

        public SharedAccountsController(ISharedAccountsService service)
        {
            this._service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        {
            IEnumerable<SharedAccountResponse> results = await this._service.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return base.Ok(results);
        }

        [HttpGet("{id:guid}")]
        [ActionName(nameof(GetAsync))]
        public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            SharedAccountResponse result = await this._service.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return base.NotFound();
            return base.Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(SharedAccountRequest request, CancellationToken cancellationToken)
        {
            SharedAccountResponse result = await this._service.CreateAsync(request, cancellationToken).ConfigureAwait(false);
            return base.CreatedAtAction(nameof(GetAsync), new { id = result.ID }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, SharedAccountRequest request, CancellationToken cancellationToken)
        {
            SharedAccountResponse result = await this._service.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false);
            return base.Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            await this._service.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            return base.NoContent();
        }
    }
}
