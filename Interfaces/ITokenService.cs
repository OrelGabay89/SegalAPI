using SegalAPI.Models;

namespace SegalAPI.Interfaces
{
    public interface ITokenService
    {
        void RedirectUserForAuthorization();
        Task<string> GetAccessToken(string authorizationCode);
        Task<Token> RefreshAccessToken();
        Task<string> GetInvoiceNumber(InvoiceCSVData data);
        string GetRefreshToken();
        int GetTokenExpiration();
        void SetAccessToken(string accessToken, string refreshToken);

    }
}