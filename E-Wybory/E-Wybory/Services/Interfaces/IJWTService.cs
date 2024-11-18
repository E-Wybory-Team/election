using E_Wybory.Infrastructure.DbContext;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace E_Wybory.Services
{
    public interface IJWTService
    {
        void CreateRSAPrivateKey() { }
        Task<string> CreateToken(RSA rsaPrivateKey, string username, ElectionDbContext context);
        JsonWebKey CreateJwkPublic(RSA rsaPrivateKey);
        JsonWebKey CreateJwkPrivate(RSA rsaPrivateKey);

        Task<string> RenewToken(JsonWebToken tokenToRenew);

        Task<string> RenewTokenClaims(string username, ElectionDbContext context, int idUserType);

        Task<string> TwoFaVeryfiedToken(string username, ElectionDbContext context, int idUserType, bool is2FAveryfied);

    }
}