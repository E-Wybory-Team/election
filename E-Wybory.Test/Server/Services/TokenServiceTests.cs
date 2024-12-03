using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using E_Wybory.Services;
using E_Wybory.Test.Server.Utils;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace E_Wybory.Test.Server.Services
{
    public class TokenServiceTests
    {


        private readonly Mock<ElectionDbContext> _mockDbContext;
        private readonly Mock<TokenValidationParameters> _mockValidationParameters;
        private readonly RSA _rsaKey;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _mockDbContext = new Mock<ElectionDbContext>();
            _mockValidationParameters = new Mock<TokenValidationParameters>();
            _rsaKey = RSA.Create();
            _tokenService = new TokenService(_rsaKey, _mockValidationParameters.Object);

        }

        private IDictionary<string, object> ExtractPayloadFromToken(string token)
        {
            var handler = new JsonWebTokenHandler();
            var jwtToken = handler.ReadJsonWebToken(token);
            var claimsDictionary = new Dictionary<string, object>();

            foreach (var claim in jwtToken.Claims)
            {
                if (claimsDictionary.ContainsKey(claim.Type))
                {
                    if (claim.Type == "Roles")
                    {
                        claimsDictionary[claim.Type] = $"{claimsDictionary[claim.Type]},{claim.Value}";
                    }
                }
                else
                {
                    claimsDictionary.Add(claim.Type, claim.Value);
                }
            }

            return claimsDictionary;
        }

        private bool ValidateToken(string token, RSA rsaKey)
        {
            var handler = new JsonWebTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "e-wybory.gov.pl",
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new RsaSecurityKey(rsaKey)
            };

            try
            {
                TokenValidationResult validationResult =  handler.ValidateTokenAsync(token, validationParameters).Result;
                Console.WriteLine(validationResult.Exception);
                return validationResult.IsValid;
            }
            catch
            {
                return false;
            }
        }

        [Fact]
        public async Task CreateToken_ShouldReturnToken()
        {
            // Arrange
            var username = "testuser";
            var electionUser = new ElectionUser { Email = username, Is2Faenabled = false, IdElectionUser = 1, IdDistrict = 1 };
            var userTypeSet = new UserTypeSet
            {
                IdElectionUser = 1,
                IdUserType = 1,
                IdUserTypeNavigation = new UserType
                {
                    IdUserType = 1,
                    IdUserTypesGroupNavigation = new UserTypesGroup { UserTypesGroupName = "GroupRole" }
                }
            };

            var mockSetUsers = MockDbSet.CreateMockDbSet<ElectionUser>(new List<ElectionUser> { electionUser }.AsQueryable());
            var mockSetRoles = MockDbSet.CreateMockDbSet(new List<UserTypeSet> { userTypeSet }.AsQueryable());

            _mockDbContext.Setup(c => c.ElectionUsers).Returns(mockSetUsers.Object);
            _mockDbContext.Setup(c => c.UserTypeSets).Returns(mockSetRoles.Object);

            // Act
            var token = await _tokenService.CreateToken(_rsaKey, username, _mockDbContext.Object);

            // Assert
            Assert.NotNull(token);

            var payload = ExtractPayloadFromToken(token);
            Assert.NotNull(payload);
            Assert.Equal("testuser", payload["name"]);
            Assert.Equal("GroupRole", payload["Roles"]);

            var isValid = ValidateToken(token, _rsaKey);
            Assert.True(isValid);
        }

        [Fact]
        public async Task RenewTokenClaims_ShouldReturnNewToken()
        {
            // Arrange
            var username = "testuser";
            var idUserType = 1;
            var electionUser = new ElectionUser { Email = username, Is2Faenabled = false, IdElectionUser = 1, IdDistrict = 1 };
            var userTypeSet = new UserTypeSet
            {
                IdElectionUser = 1,
                IdUserType = 1,
                IdUserTypeNavigation = new UserType
                {
                    IdUserType = 1,
                    IdUserTypesGroupNavigation = new UserTypesGroup { UserTypesGroupName = "GroupRole" }
                }
            };

            var mockSetUsers = MockDbSet.CreateMockDbSet<ElectionUser>(new List<ElectionUser> { electionUser }.AsQueryable());
            var mockSetRoles = MockDbSet.CreateMockDbSet(new List<UserTypeSet> { userTypeSet }.AsQueryable());

            _mockDbContext.Setup(c => c.ElectionUsers).Returns(mockSetUsers.Object);
            _mockDbContext.Setup(c => c.UserTypeSets).Returns(mockSetRoles.Object);

            // Act
            var newToken = await _tokenService.RenewTokenClaims(username, _mockDbContext.Object, idUserType);

            // Assert
            Assert.NotNull(newToken);

            var payload = ExtractPayloadFromToken(newToken);

            Assert.NotNull(payload);
            Assert.Equal("testuser", payload["name"]);
            Assert.Equal("GroupRole", payload["Roles"]);

            var isValid = ValidateToken(newToken, _rsaKey);
            Assert.True(isValid);
        }

        [Fact]
        public async Task TwoFaVeryfiedToken_ShouldReturnNewToken()
        {
            // Arrange
            var username = "testuser";
            var idUserType = 1;
            var is2FAveryfied = true;
            var electionUser = new ElectionUser { Email = username, Is2Faenabled = true, IdElectionUser = 1, IdDistrict = 1 };
            var userTypeSet = new UserTypeSet
            {
                IdElectionUser = 1,
                IdUserType = 1,
                IdUserTypeNavigation = new UserType
                {
                    IdUserType = 1,
                    IdUserTypesGroupNavigation = new UserTypesGroup { UserTypesGroupName = "GroupRole" }
                }
            };

            var mockSetUsers = MockDbSet.CreateMockDbSet<ElectionUser>(new List<ElectionUser> { electionUser }.AsQueryable());
            var mockSetRoles = MockDbSet.CreateMockDbSet(new List<UserTypeSet> { userTypeSet }.AsQueryable());

            _mockDbContext.Setup(c => c.ElectionUsers).Returns(mockSetUsers.Object);
            _mockDbContext.Setup(c => c.UserTypeSets).Returns(mockSetRoles.Object);

            // Act
            var newToken = await _tokenService.TwoFaVeryfiedToken(username, _mockDbContext.Object, idUserType, is2FAveryfied);

            // Assert
            Assert.NotNull(newToken);

            var payload = ExtractPayloadFromToken(newToken);

            Assert.NotNull(payload);
            Assert.Equal("testuser", payload["name"]);
            Assert.Equal("true", payload["2FAenabled"]);
            Assert.Equal("1", payload["IdElectionUser"]);
            Assert.Equal("1", payload["IdDistrict"]);
            Assert.Equal("1", payload["IdUserType"]);
            Assert.Contains("GroupRole", payload["Roles"].ToString());
            Assert.Contains("2FAveryfiedUser", payload["Roles"].ToString());

            var isValid = ValidateToken(newToken, _rsaKey);
            Assert.True(isValid);
        }
    }
}
