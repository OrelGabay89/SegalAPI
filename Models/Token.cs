using Newtonsoft.Json;

namespace SegalAPI.Models
{
    public class Token
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }
}
