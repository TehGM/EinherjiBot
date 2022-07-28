using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TehGM.EinherjiBot.Security.API.Controllers
{
    [Route("api/auth/token")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            this._service = service;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            LoginResponse response = await this._service.LoginAsync(request.DiscordCode, cancellationToken).ConfigureAwait(false);
            if (response == null)
                return base.Unauthorized();

            return base.Ok(response);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
        {
            LoginResponse response = await this._service.RefreshAsync(request.RefreshToken, cancellationToken).ConfigureAwait(false);
            if (response == null)
                return base.Unauthorized();

            return base.Ok(response);
        }

        [HttpDelete]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAsync(RefreshRequest request, CancellationToken cancellationToken = default)
        {
            await this._service.LogoutAsync(request.RefreshToken, cancellationToken).ConfigureAwait(false);
            return base.Ok();
        }
    }
}
