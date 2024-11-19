using E_Wybory.Application.DTOs;
using E_Wybory.Client.ViewModels;
namespace E_Wybory.Client.Services
{
    public interface IAuthService
    {
        Task<bool> Login(LoginViewModel login);

        Task<bool> Register(RegisterViewModel register);

        Task<bool> Logout();

        Task<bool> RenewTokenClaims(UserInfoViewModel userInfo);

        Task<bool> VerifyTwoFactorTokenAsyncFirst(TwoFactorAuthVerifyRequest ver2fa);

        Task<bool> VerifyTwoFactorTokenAsync(TwoFactorAuthVerifyRequest ver2fa);

        Task<int> CountRecoveryCodesAsync(int userId);

       //Task<bool> SetTwoFactorEnabledAsync(int userId, bool enabled);

        Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(int userId);

        Task<string> GetAuthenticatorKeyAsync(int userId);

        Task<bool> Reset2FAasync(int userId);

        public Task<bool> ForgetPassword(ForgetPasswordViewModel forgetPassword);

        public Task<bool> ResetPassword(ResetPasswordViewModel resetPassword);



    }
}
