using E_Wybory.Infrastructure.DbContext;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace E_Wybory.Services
{
    public interface IJWTService
    {
        static void CreateRSAPrivateKey() { }
        static string createToken(RSA rsaPrivateKey, string username) { return null; }
        static JsonWebKey CreateJwkPublic(RSA rsaPrivateKey) { return null; }
        static JsonWebKey CreateJwkPrivate(RSA rsaPrivateKey) { return null; }

    }
}
