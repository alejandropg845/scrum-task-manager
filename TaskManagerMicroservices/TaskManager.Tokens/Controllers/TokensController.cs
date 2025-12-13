using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Tokens.Interfaces;

namespace TaskManager.Tokens.Controllers
{
    [ApiController]
    [Route("api/tokens")]
    public class TokensController : ControllerBase
    {
        private readonly ITokensService _service;
        public TokensController(ITokensService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("GetAccessToken/{refreshToken}")]
        public async Task<ActionResult> GetNewAccessToken(string refreshToken)
        {
            var r = await _service.GetNewAccessTokenAsync(refreshToken);

            return Ok(new { r.AccessToken, r.RefreshToken });

        }

        [HttpPost("SaveRefreshToken")]
        [Authorize]
        public async Task<ActionResult> SaveRefreshToken([FromBody] Token token)
        {
            var createdRToken = await _service.SaveRefreshTokenAsync(token);
            return Ok(createdRToken);
        }
    }
}
