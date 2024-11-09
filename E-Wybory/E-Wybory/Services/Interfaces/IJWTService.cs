using E_Wybory.Infrastructure.DbContext;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace E_Wybory.Services
{
    public interface IJWTService
    {
        void CreateRSAPrivateKey() { }
        Task<string> CreateToken(RSA rsaPrivateKey, string username, ElectionDbContext context) { return null; }
        JsonWebKey CreateJwkPublic(RSA rsaPrivateKey) { return null; }
        JsonWebKey CreateJwkPrivate(RSA rsaPrivateKey) { return null; }

    }
}