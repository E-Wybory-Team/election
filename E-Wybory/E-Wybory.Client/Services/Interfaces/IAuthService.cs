using E_Wybory.Client.ViewModels;
namespace E_Wybory.Client.Services
{
    public interface IAuthService
    {
        Task<bool> Login(LoginViewModel login);

        Task<bool> Register(RegisterViewModel register);

        Task<bool> Logout();

        Task<bool> RenewTokenClaims(UserInfoViewModel userInfo);

        Task<bool> VerifyTwoFactorTokenAsync(int userId, string code);

        Task<int> CountRecoveryCodesAsync(int userId);

        Task<bool> SetTwoFactorEnabledAsync(int userId, bool enabled);

        Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(int userId);

        Task<string> GetAuthenticatorKeyAsync(int userId);

        Task<string> ResetAuthenticatorKeyAsync(int userId);

    }
}
