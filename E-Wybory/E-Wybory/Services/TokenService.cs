﻿using E_Wybory.Domain.Entities;
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
using ZstdSharp.Unsafe;

namespace E_Wybory.Services
{
    public class TokenService : IJWTService
    {
        const int JWT_TOKEN_VALIDATION_MINS = 10;
        const string privateKeyFileName = "../key";
        private readonly TokenValidationParameters _validationParameters;

        public static RSA rsaKey { get; set; }

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
                Issuer = "e-wybory.gov.pl",
                Subject = await claimsIdentity,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
                Expires = DateTime.UtcNow.AddMinutes(JWT_TOKEN_VALIDATION_MINS),
                IssuedAt = DateTime.UtcNow
            }); 
            return token;
        }


        private async Task<ClaimsIdentity> GenerateClaims(string username, ElectionDbContext context, int? idUserType = null, bool? twoFaVeryfied = null)
        {
            var electionUser = await context.ElectionUsers.Where(e => e.Email.Equals(username)).FirstOrDefaultAsync();

            if(electionUser is null) return new ClaimsIdentity();

            var claims = new List<Claim>()
            {
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("name", username),
                electionUser.Is2Faenabled ? new Claim("2FAenabled", "true") : new Claim("2FAdisabled", "true"),
                new Claim("IdElectionUser", electionUser.IdElectionUser.ToString()),
                new Claim("IdDistrict", electionUser.IdDistrict.ToString()),
            };

            if (electionUser.Is2Faenabled && (twoFaVeryfied.HasValue && twoFaVeryfied.Value))
                claims.Add(new Claim("Roles", "2FAveryfiedUser"));

            var userTypeSet = await GetRole(electionUser.IdElectionUser, context, idUserType);

            if(userTypeSet is not null)
            {
                claims.Add(new Claim("Roles", userTypeSet.IdUserTypeNavigation.IdUserTypesGroupNavigation.UserTypesGroupName));
                claims.Add(new Claim("IdUserType", userTypeSet.IdUserType.ToString()));
            } else
            {
                claims.Add(new Claim("IdUserType", "0"));
            }

            return new ClaimsIdentity(claims);
        }

        public async Task<UserTypeSet?> GetRole(int idElectionUser, ElectionDbContext context, int? idUserType = null)
        {
            var userTypeSet = await context.UserTypeSets
                .Include(u => u.IdElectionUserNavigation)
                .Include(u => u.IdUserTypeNavigation.IdUserTypesGroupNavigation)
                .Where(u => u.IdElectionUser == idElectionUser && (idUserType == null || 
                        u.IdUserTypeNavigation.IdUserType == idUserType))
                .OrderBy(u => u.IdUserTypeSet)
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

        public async Task<string> RenewTokenClaims(string username, ElectionDbContext context, int idUserType)
        {
            return await CreateToken(rsaKey, GenerateClaims(username, context, idUserType));
        }

        public async Task<string> TwoFaVeryfiedToken(string username, ElectionDbContext context, int idUserType, bool is2FAveryfied)
        {
            return await CreateToken(rsaKey, GenerateClaims(username, context, idUserType: idUserType, twoFaVeryfied: is2FAveryfied));
        }
    }
}
