
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SegalAPI.Data;
using SegalAPI.Interfaces;
using SegalAPI.Models;
using SegalAPI.Services;

namespace SegalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        public InvoiceController(ILogger<InvoiceController> logger, IConfiguration configuration, ITokenService tokenService)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpGet("_alive")]
        public IActionResult IsAlive()
        {
            return Ok("API is up and running!");
        }

        [HttpPost("process-data")]
        public async Task<IActionResult> ProcessData([FromBody] InvoiceCSVData data)
        {
            if (data == null)
            {
                _logger.LogError("Received null data");
                return BadRequest("Data cannot be null");
            }

            // Assuming the authorization code is obtained and stored elsewhere
            string authorizationCode = "obtained_authorization_code";

            // Step 3: Get access token using authorization code
            string accessToken = await _tokenService.GetAccessToken(authorizationCode);

            // Step 5: Use access token to get invoice number
            await _tokenService.GetInvoiceNumber(accessToken);

            _logger.LogInformation($"Data processed for {data.Id}");
            return Ok($"Data received with ID: {data.Id}");
        }
    }
}