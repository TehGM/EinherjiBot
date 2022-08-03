using Microsoft.AspNetCore.Mvc;
using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.PlaceholdersEngine.API.Controllers
{
    [Route("api/placeholders")]
    [ApiController]
    public class PlaceholdersController : ControllerBase
    {
        private readonly IPlaceholdersService _service;

        public PlaceholdersController(IPlaceholdersService service)
        {
            this._service = service;
        }

        [HttpPost("convert")]
        [Authorize]
        public async Task<IActionResult> ConvertAsync(PlaceholdersConvertRequest request, CancellationToken cancellationToken)
        {
            PlaceholdersConvertResponse response = await this._service.ConvertAsync(request, cancellationToken).ConfigureAwait(false);
            return Ok(response);
        }
    }
}
