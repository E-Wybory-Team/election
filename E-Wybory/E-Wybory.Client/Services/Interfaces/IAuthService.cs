using E_Wybory.Client.ViewModels;
namespace E_Wybory.Client.Services
{
    public interface IAuthService
    {
        Task<bool> Login(LoginViewModel login);

        Task<bool> Register(RegisterViewModel register);

        Task<bool> Logout();

        Task<bool> RenewTokenClaims(UserInfoViewModel userInfo);
    }
}
