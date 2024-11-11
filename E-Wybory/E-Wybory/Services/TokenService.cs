using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;

namespace E_Wybory.Services
{
    public class TokenService : IJWTService
    {
        const int JWT_TOKEN_VALIDATION_MINS = 10;//1;
        const string privateKeyFileName = "../key";
        private readonly TokenValidationParameters _validationParameters;

        public static RSA rsaKey { get; set; }

        //IServiceScopeFactory sopeFactory
        //https://stackoverflow.com/questions/36332239/use-dbcontext-in-asp-net-singleton-injected-class

        public TokenService(RSA key, TokenValidationParameters tokenValidationParameters)
        {
            rsaKey ??= key;
            rsaKey.ImportRSAPrivateKey(System.IO.File.ReadAllBytes("key"), out _);
            _validationParameters = tokenValidationParameters;
            _validationParameters.IssuerSigningKey = new RsaSecurityKey(rsaKey);
        }


        public void CreateRSAPrivateKey()
        {
            var privateKey = rsaKey.ExportRSAPrivateKey();
            File.WriteAllBytes(privateKeyFileName, privateKey);
        }

        public async Task<string> CreateToken(RSA rsaPrivateKey, string username, ElectionDbContext context)
        {
            return await CreateToken(rsaPrivateKey, GenerateClaims(username, context));
        }

        private async Task<string> CreateToken(IEnumerable<Claim> claims)
        {
            return await CreateToken(rsaKey, Task.FromResult(new ClaimsIdentity(claims)));
        }

        private async Task<string> CreateToken(RSA rsaPrivateKey, Task<ClaimsIdentity> claimsIdentity)
        {
            var handler = new JsonWebTokenHandler();
            var key = new RsaSecurityKey(rsaPrivateKey);
            var token = handler.CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = "https://localhost:8443",
                Subject = await claimsIdentity,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
                Expires = DateTime.UtcNow.AddMinutes(JWT_TOKEN_VALIDATION_MINS),
                IssuedAt = DateTime.UtcNow
            }); //as JwtSecurityToken;
            //return token?.RawData ?? string.Empty;
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

            var userTypeSet = await GetRole(electionUser.IdElectionUser, context, idUserType);

            if(userTypeSet is not null)
            {
                claims.Add(new Claim("Roles", userTypeSet.IdUserTypeNavigation.IdUserTypesGroupNavigation.UserTypesGroupName));
                claims.Add(new Claim("IdUserType", userTypeSet.IdUserType.ToString()));
            }

            return new ClaimsIdentity(claims);
        }

        public async Task<UserTypeSet?> GetRole(int idElectionUser, ElectionDbContext context, int idUserType = 0)
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

        public async Task<string> RenewToken(JsonWebToken tokenToRenew)
        {
           
                var handler = new JsonWebTokenHandler();
                TokenValidationResult validationResult = await handler.ValidateTokenAsync(tokenToRenew.EncodedToken, _validationParameters);

                if (validationResult.IsValid)
                {
                   
                        var newToken = await CreateToken(tokenToRenew.Claims);
                        return newToken;
                    
                } else
                {
                    Console.WriteLine(validationResult.Exception.Message);
                }
           

            return null;
        }
    }
}
