using E_Wybory.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace E_Wybory.Controllers
{
    public class AuthController : ControllerBase
    {
        public static ElectionUser user;
        private readonly IConfiguration configuration;

        public AuthController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


    }
}
