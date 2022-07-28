using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TehGM.EinherjiBot.API.Controllers
{
    [Route("api/user/info")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly IUserInfoService _service;

        public UserInfoController(IUserInfoService service)
        {
            this._service = service;
        }

        [HttpGet("bot")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBotInfoAsync(CancellationToken cancellationToken)
        {
            UserInfoResponse response = await this._service.GetBotInfoAsync(cancellationToken).ConfigureAwait(false);
            return base.Ok(response);
        }

        [HttpGet("{id:ulong}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserInfoAsync(ulong id, CancellationToken cancellationToken)
        {
            UserInfoResponse response = await this._service.GetUserInfoAsync(id, cancellationToken).ConfigureAwait(false);
            if (response == null)
                return base.NotFound();
            return base.Ok(response);
        }

        [HttpGet]
        public Task<IActionResult> GetCurrentUserInfoAsync(CancellationToken cancellationToken)
        {
            IDiscordAuthContext context = base.HttpContext.Features.Get<IDiscordAuthContext>();
            if (context == null)
                return Task.FromResult((IActionResult)base.Unauthorized());
            return this.GetUserInfoAsync(context.ID, cancellationToken);
        }
    }
}
