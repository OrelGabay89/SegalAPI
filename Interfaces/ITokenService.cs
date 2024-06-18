namespace SegalAPI.Interfaces
{
    public interface ITokenService
    {
        void RedirectUserForAuthorization();
        Task<string> GetAccessToken(string authorizationCode);
        Task<string> RefreshAccessToken();
        Task<string> GetInvoiceNumber();
        string GetRefreshToken();
        int GetTokenExpiration();
        void SetAccessToken(string accessToken);

    }
}