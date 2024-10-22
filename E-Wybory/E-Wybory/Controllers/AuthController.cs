using E_Wybory.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Wybory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public IActionResult Login([FromForm] AuthenticationRequest request)
        {
            var authResult = JWTMethods.Authenticate(request.email, request.password);
            if (authResult == null)
                return Unauthorized();

            return Ok(authResult);
        }
    }
}
