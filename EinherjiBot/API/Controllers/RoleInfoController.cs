using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TehGM.EinherjiBot.API.Controllers
{
    [Route("api/role/info")]
    [ApiController]
    public class RoleInfoController : ControllerBase
    {
        private readonly IRoleInfoService _service;

        public RoleInfoController(IRoleInfoService service)
        {
            this._service = service;
        }

        [HttpGet("{id:ulong}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserInfoAsync(ulong id, CancellationToken cancellationToken)
        {
            RoleInfoResponse response = await this._service.GetRoleInfoAsync(id, cancellationToken).ConfigureAwait(false);
            if (response == null)
                return base.NotFound();
            return base.Ok(response);
        }
    }
}
