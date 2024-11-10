using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
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
            rsaKey ??= key;
        }


        public void CreateRSAPrivateKey()
        {
            var privateKey = rsaKey.ExportRSAPrivateKey();
            File.WriteAllBytes(privateKeyFileName, privateKey);
        }

        public async Task<string> CreateToken(RSA rsaPrivateKey, string username, ElectionDbContext context)
        {
            var handler = new JsonWebTokenHandler();
            var key = new RsaSecurityKey(rsaPrivateKey);
            var token = handler.CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = "https://localhost:8443",
            //    Subject = new ClaimsIdentity(new[]
            //    {
            //new Claim("sub", Guid.NewGuid().ToString()),
            //new Claim("name", username),
            //new Claim("Roles", "Administrator") //Added for testing

            //    }),
                Subject = await GenerateClaims(username, context),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
                Expires = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDATION_MINS)
            });
            return token;
        }

        public async Task<ClaimsIdentity> GenerateClaims(string username, ElectionDbContext context, int idUserType = 0)
        {
            var electionUser = await context.ElectionUsers.Where(e => e.Email.Equals(username)).FirstOrDefaultAsync();

            if(electionUser is null) return new ClaimsIdentity();

            var claims = new List<Claim>() 
            {
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("name", username),
                //new Claim("Roles", "Wyborca"), //By default
                new Claim("IdElectionUser", electionUser.IdElectionUser.ToString()),
                new Claim("IdDistrict", electionUser.IdDistrict.ToString()),
            };

            var userTypeSet = await GetRole(context, electionUser.IdElectionUser, idUserType);

            if(userTypeSet is not null)
            {
                claims.Add(new Claim("Roles", userTypeSet.IdUserTypeNavigation.IdUserTypesGroupNavigation.UserTypesGroupName));
                claims.Add(new Claim("IdUserType", userTypeSet.IdUserType.ToString()));
            }

            return new ClaimsIdentity(claims);
        }

        public async Task<UserTypeSet?> GetRole(ElectionDbContext context, int idElectionUser, int idUserType = 0)
        {
            var userTypeSet = await context.UserTypeSets
                .Include(u => u.IdElectionUserNavigation)
                .Include(u => u.IdUserTypeNavigation.IdUserTypesGroupNavigation)
                .Where(u => u.IdElectionUser == idElectionUser && (idUserType <= 0 || 
                        u.IdUserTypeNavigation.IdUserType == idUserType))
                .OrderBy(u => u.IdUserTypeSet) //How to get default user role? "Wyborca" by deafult?
                .FirstOrDefaultAsync();

            return userTypeSet;
        }

        public JsonWebKey CreateJwkPublic(RSA rsaPrivateKey)
        {
            var publicKey = RSA.Create();
            publicKey.ImportRSAPublicKey(rsaPrivateKey.ExportRSAPublicKey(), out _);

            var key = new RsaSecurityKey(publicKey);
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        }

        public JsonWebKey CreateJwkPrivate(RSA rsaPrivateKey)
        {
            var key = new RsaSecurityKey(rsaPrivateKey);
            return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        }
    }
}