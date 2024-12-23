﻿using E_Wybory.Application.DTOs;
using E_Wybory.Client.ViewModels;
namespace E_Wybory.Client.Services
{
    public interface IAuthService
    {
        Task<string> Login(LoginViewModel login);

        Task<bool> Register(RegisterViewModel register);

        Task<bool> Logout();

        Task<bool> RenewTokenClaims(UserInfoViewModel userInfo);

        Task<bool> VerifyTwoFactorTokenAsyncFirst(TwoFactorAuthVerifyRequest ver2fa);

        Task<bool> VerifyTwoFactorTokenAsync(TwoFactorAuthVerifyRequest ver2fa);

        Task<int> CountRecoveryCodesAsync(int userId);

        Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(int userId);

        Task<string> GetAuthenticatorKeyAsync(int userId);

        Task<bool> Reset2FAasync(int userId);

        public Task<bool> ForgetPassword(ForgetPasswordViewModel forgetPassword);
        public Task<bool> SendingConfirmation();

        public Task<bool> ResetPassword(ResetPasswordViewModel resetPassword);
        Task<int> GetCurrentUserIdDistrict();
        Task<int> GetCurrentUserIdVoter();
        Task<bool> GetCurrentUser2faStatus();
        Task<int> GetCurrentVoterIdDistrict();


    }
}
