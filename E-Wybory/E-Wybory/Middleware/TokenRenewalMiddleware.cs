using E_Wybory.Infrastructure.DbContext;
using E_Wybory.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;

namespace E_Wybory.Middleware
{
    public class TokenRenewalMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IJWTService _tokenService;
        private readonly ElectionDbContext _context;


        public TokenRenewalMiddleware(RequestDelegate next, IJWTService tokenService)
        {
            _next = next;
            _tokenService = tokenService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            

            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JsonWebTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJsonWebToken(token);
                    var expirationTime = jwtToken.ValidTo; //Are datetime.min
                    var issuedAtTime = jwtToken.ValidFrom;

                    if (expirationTime > DateTime.UtcNow)
                    {
                        var tokenLifespan = expirationTime - issuedAtTime;
                        var seventyPercentThreshold = issuedAtTime.AddSeconds(tokenLifespan.TotalSeconds * 0.7);

                        if (DateTime.UtcNow >= seventyPercentThreshold)
                        {
                            var newToken = await _tokenService.RenewToken(jwtToken);

                            if(newToken is not null)
                            // Add the renewed token to the response header
                            context.Response.Headers["Authorization"] = $"Bearer {newToken}";
                        } 
                        else
                        {
                            //context.Response.Redirect("/login"); //Does it work?
                        }
                    }
                }
            }

            

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

}
