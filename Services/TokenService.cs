using SegalAPI.Interfaces;
using SegalAPI.Data;
using SegalAPI.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace SegalAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string redirectUri;
        private readonly string authorizationUrl;
        private readonly string tokenUrl;

        private static string _accessToken;
        private static string _refreshToken;
        private static int _tokenExpiration;

        public TokenService(HttpClient httpClient, IConfiguration configuration, AppDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;

            clientId = _configuration["OAuth:ClientId"];
            clientSecret = _configuration["OAuth:ClientSecret"];
            redirectUri = _configuration["OAuth:RedirectUri"];
            authorizationUrl = _configuration["OAuth:AuthorizationUrl"];
            tokenUrl = _configuration["OAuth:TokenUrl"];
        }

        public void RedirectUserForAuthorization()
        {
            string url = $"{authorizationUrl}?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&scope=scope&state=state";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        public async Task<string> GetAccessToken(string authorizationCode)
        {
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("scope", "scope"),
        });

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = content
            };

            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            // הדפסת פרטי הבקשה
            Console.WriteLine("Request Headers:");
            foreach (var header in request.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            Console.WriteLine("Request Content Headers:");
            foreach (var header in request.Content.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            Console.WriteLine("Request Body:");
            Console.WriteLine(await request.Content.ReadAsStringAsync());

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            // הדפסת סטטוס קוד ופרטי התשובה
            Console.WriteLine("Response Status Code:");
            Console.WriteLine(response.StatusCode);

            Console.WriteLine("Response Headers:");
            foreach (var header in response.Headers)
            {
                Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response Body:");
            Console.WriteLine(responseContent);

            if (response.IsSuccessStatusCode)
            {
                JObject tokenResponse = JObject.Parse(responseContent);
                _accessToken = tokenResponse["access_token"].ToString();
                _refreshToken = tokenResponse["refresh_token"].ToString();
                _tokenExpiration = tokenResponse["expires_in"].ToObject<int>();

                // Save tokens to database
                var token = new Token
                {
                    AccessToken = _accessToken,
                    RefreshToken = _refreshToken,
                    Expiration = DateTime.UtcNow.AddSeconds(_tokenExpiration)
                };

                // Assuming _context is an instance of your database context
                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();

                return _accessToken;
            }
            else
            {
                // Handle error response
                Console.WriteLine("Error Response:");
                Console.WriteLine(responseContent);
                throw new Exception($"Error retrieving access token: {response.StatusCode}");
            }
        }


        public async Task<Token> RefreshAccessToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", _refreshToken),
                new KeyValuePair<string, string>("scope", "scope"),
            });

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            JObject tokenResponse = JObject.Parse(responseContent);
            var newAccessToken = tokenResponse["access_token"].ToString();
            var newRefreshToken = tokenResponse["refresh_token"].ToString();

            var newExpiration = tokenResponse["expires_in"].ToObject<int>();
            var expirationDateTime = DateTime.Now.AddSeconds(newExpiration);

            var token = new Token { AccessToken = newAccessToken, RefreshToken = newRefreshToken, Expiration = expirationDateTime };
            if (token != null)
            {
                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();
            }

            return token;
        }

        public async Task<string> GetInvoiceNumber(InvoiceCSVData data)
        {
            try
            {
                // Ensure we have a valid access token
                if (string.IsNullOrEmpty(_accessToken))
                {
                    throw new Exception("Access token is not available.");
                }

                return await GetInvoiceNumberWithAccessToken(data);
            }
            catch (Exception ex)
            {
                await RefreshAccessToken();
                return await GetInvoiceNumberWithAccessToken(data);


                // Handle other exceptions
                throw new Exception("Error getting invoice number", ex);
            }
        }

        private async Task<string> GetInvoiceNumberWithAccessToken(InvoiceCSVData data)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);


            var invoiceRequest = new
            {
                data.Invoice_ID,
                data.Invoice_Type,
                data.Vat_Number,
                data.Union_Vat_Number,
                data.Invoice_Reference_Number,
                data.Customer_VAT_Number,
                data.Customer_Name,
                Invoice_Date = data.Invoice_Date.ToString("yyyy-MM-dd"),
                Invoice_Issuance_Date = data.Invoice_Issuance_Date.ToString("yyyy-MM-dd"),
                data.Branch_ID,
                data.Accounting_Software_Number,
                data.Client_Software_Key,
                data.Amount_Before_Discount,
                data.Discount,
                data.Payment_Amount,
                data.VAT_Amount,
                data.Payment_Amount_Including_VAT,
                data.Invoice_Note,
                data.Action,
                data.Vehicle_License_Number,
                data.Transition_Location,
                data.Delivery_Address,
                data.Additional_Information,
                Items = data.Items.Select(item => new
                {
                    item.Index,
                    item.Catalog_ID,
                    item.Description,
                    item.Measure_Unit_Description,
                    item.Quantity,
                    item.Price_Per_Unit,
                    item.Discount,
                    item.Total_Amount,
                    item.VAT_Rate,
                    item.VAT_Amount
                }).ToArray()
            };


            var request = new HttpRequestMessage(HttpMethod.Post, "https://openapi.taxes.gov.il/shaam/sandbox/longtimeacces");
            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(invoiceRequest), System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            JObject invoiceResponse = JObject.Parse(responseContent);
            return invoiceResponse["Invoice_Allocation_Number"].ToString();
        }

        public string GetRefreshToken()
        {
            return _refreshToken;
        }

        public int GetTokenExpiration()
        {
            return _tokenExpiration;
        }

        public void SetAccessToken(string accessToken, string refreshToken)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
        }
    }
}
