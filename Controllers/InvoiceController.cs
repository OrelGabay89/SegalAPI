using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SegalAPI.Interfaces;
using SegalAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace SegalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public InvoiceController(ILogger<InvoiceController> logger, ITokenService tokenService, AppDbContext context)
        {
            _logger = logger;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpGet("_alive")]
        public IActionResult IsAlive()
        {
            return Ok("API is up and running!");
        }

        [HttpPost("process-data")]
        public async Task<IActionResult> ProcessData()
        {
            try
            {
                // Retrieve the most recent token from the database
                var token = await _context.Tokens.OrderByDescending(t => t.Id).FirstOrDefaultAsync();

                if (token == null || token.Expiration <= DateTime.UtcNow)
                {
                    // If the token is null or expired, refresh it
                    string newAccessToken = await _tokenService.RefreshAccessToken();
                    token.AccessToken = newAccessToken;
                    token.Expiration = DateTime.UtcNow.AddSeconds(_tokenService.GetTokenExpiration());

                    _context.Tokens.Update(token);
                    await _context.SaveChangesAsync();
                }

                // Use the access token to get the invoice number
                _tokenService.SetAccessToken(token.AccessToken);
                string invoiceNumber = await _tokenService.GetInvoiceNumber();
                return Ok(invoiceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
