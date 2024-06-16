namespace SegalAPI.Interfaces
{
    public interface ITokenService
    {
        void RedirectUserForAuthorization();
        Task<string> GetAccessToken(string authorizationCode);
        Task<string> RefreshAccessToken(string refreshToken);
        Task<string> GetInvoiceNumber(string accessToken);

    }
}