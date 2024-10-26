using E_Wybory.Client.ViewModels;
namespace E_Wybory.Client.Services
{
    public interface IAuthService
    {
        Task<string?> Login(LoginViewModel login);

        Task<bool> Register(RegisterViewModel register);
        Task<List<DistrictViewModel>> Districts();
    }
}
