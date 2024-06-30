using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SegalAPI.Interfaces;
using SegalAPI.Data;
using Microsoft.EntityFrameworkCore;
using SegalAPI.Models;

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
        public async Task<IActionResult> ProcessData([FromBody] InvoiceCSVData data)
        {
            try
            {
                // Retrieve the most recent token from the database
                var token = await _context.Tokens.OrderByDescending(t => t.Id).FirstOrDefaultAsync();

                // Use the access token to get the invoice number
                _tokenService.SetAccessToken(token.AccessToken, token.RefreshToken);

                if (token.Expiration <= DateTime.Now)
                {
                    // If the token is null or expired, refresh it
                    var newToken =  await _tokenService.RefreshAccessToken();
                    _tokenService.SetAccessToken(newToken.AccessToken, newToken.RefreshToken);
                }

                string invoiceNumber = await _tokenService.GetInvoiceNumber(data);
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
