using Microsoft.AspNetCore.Mvc;
using SegalAPI.Interfaces;
using SegalAPI.Data;
using SegalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace SegalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public OAuthController(ITokenService tokenService, AppDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpGet("_alive")]
        public IActionResult IsAlive()
        {
            return Ok("API is up and running!");
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

            string accessToken = await _tokenService.GetAccessToken(code);

            // Save the access token to the database
            var token = new Token
            {
                AccessToken = accessToken,
                RefreshToken = _tokenService.GetRefreshToken(),
                Expiration = DateTime.UtcNow.AddSeconds(_tokenService.GetTokenExpiration())
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            return Ok("Authorization code captured and access token obtained.");
        }
    }
}
