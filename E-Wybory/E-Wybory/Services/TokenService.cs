using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace E_Wybory.Services
{
    public class TokenService : IJWTService
    {
        const int JWT_TOKEN_VALIDATION_MINS = 10;
        const string privateKeyFileName = "../key";
        public static RSA rsaKey { get; set; }

        public TokenService(RSA key)
        {
            rsaKey = key;
        }


        public static void CreateRSAPrivateKey()
        {
            var privateKey = rsaKey.ExportRSAPrivateKey();
            File.WriteAllBytes(privateKeyFileName, privateKey);
        }

        public static string createToken(RSA rsaPrivateKey, string username)
        {
            var handler = new JsonWebTokenHandler();
            var key = new RsaSecurityKey(rsaPrivateKey);
            var token = handler.CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = "https://localhost:8443",
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("name", username),
            new Claim("Roles", "Administrator") //Added for testing

            }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
                Expires = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDATION_MINS)
            });
            return token;
        }

        public static JsonWebKey CreateJwkPublic(RSA rsaPrivateKey)
        {
            var publicKey = RSA.Create();
            publicKey.ImportRSAPublicKey(rsaPrivateKey.ExportRSAPublicKey(), out _);

            var key = new RsaSecurityKey(publicKey);
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        }

        public static JsonWebKey CreateJwkPrivate(RSA rsaPrivateKey)
        {
            var key = new RsaSecurityKey(rsaPrivateKey);
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        }
    }
}