
using SegalAPI.Interfaces;
using System.Net.Http.Headers;

namespace SegalAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string redirectUri;
        private readonly string authorizationUrl;
        private readonly string tokenUrl;

        public TokenService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

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
            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorizationCode)
            });

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
            return tokenResponse.access_token;
        }

        public async Task<string> RefreshAccessToken(string refreshToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
            return tokenResponse.access_token;
        }
        public async Task<string> GetInvoiceNumber(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var invoiceRequest = new
            {
                Invoice_ID = "72727890",
                Invoice_Type = 305,
                Vat_Number = 777777715,
                Union_Vat_Number = 0,
                Invoice_Reference_Number = "015345367",
                Customer_VAT_Number = 111111118,
                Customer_Name = "עלי אבוג'דידה",
                Invoice_Date = "2024-06-14",
                Invoice_Issuance_Date = "2024-06-16",
                Branch_ID = "716A",
                Accounting_Software_Number = 207703,
                Client_Software_Key = "G4255422",
                Amount_Before_Discount = 10508.08,
                Discount = 202.24,
                Payment_Amount = 10305.84,
                VAT_Amount = 1751.99,
                Payment_Amount_Including_VAT = 12057.83,
                Invoice_Note = "לתאם הספקה עם אחמד 052-9290009",
                Action = 0,
                Vehicle_License_Number = 0,
                Transition_Location = 0,
                Delivery_Address = "",
                Additional_Information = 4365,
                Items = new[]
                {
                    new { Index = 1, Catalog_ID = "אא--72729", Description = "דשא דרבן", Measure_Unit_Description = "מ\"ר", Quantity = 9.52, Price_Per_Unit = 2.89, Discount = 1.65, Total_Amount = 25.87, VAT_Rate = 17, VAT_Amount = 4.39 },
                    new { Index = 2, Catalog_ID = "אא--72730", Description = "דשא דרבן", Measure_Unit_Description = "מ\"ר", Quantity = 19.05, Price_Per_Unit = 5.78, Discount = 3.3, Total_Amount = 106.81, VAT_Rate = 17, VAT_Amount = 18.15 },
                    // Add other items here...
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "invoiceUrl");
            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(invoiceRequest), System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic invoiceResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
            return invoiceResponse.Invoice_Allocation_Number;
        }
    }
}