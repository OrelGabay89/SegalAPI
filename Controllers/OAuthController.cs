using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SegalAPI.Interfaces;

namespace SegalAPI.Controllers
{

    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public OAuthController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpGet("authorize")]
        public IActionResult Authorize()
        {
            _tokenService.RedirectUserForAuthorization();
            return Ok("Redirected to authorization URL. Please complete the login and authorization process.");
        }

        [HttpGet("success")]
        public async Task<IActionResult> CaptureAuthorizationCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code not found.");
            }

            //string accessToken = await _tokenService.GetAccessToken(code);

            // Save or use the access token as needed
            // ...

            return Ok("Authorization code captured and access token obtained.");
        }
    }
}