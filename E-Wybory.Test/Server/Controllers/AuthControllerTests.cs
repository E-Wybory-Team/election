using Azure.Communication.Email;
using E_Wybory.Application.DTOs;
using E_Wybory.Client.ViewModels;
using E_Wybory.Controllers;
using E_Wybory.Domain.Entities;
using E_Wybory.Infrastructure.DbContext;
using E_Wybory.Services;
using E_Wybory.Services.Interfaces;
using E_Wybory.Test.Server.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NuGet.Versioning;
using OtpNet;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace E_Wybory.Test.Server.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ElectionDbContext> _mockContext;
        private readonly Mock<IJWTService> _mockTokenService;
        private readonly Mock<IEmailSenderService> _mockEmailSenderService;
        private readonly AuthController _controller;

        private static string HashPassword(string password)
        {
            SHA256 sha = SHA256.Create();
            UTF8Encoding objUtf8 = new UTF8Encoding();
            byte[] hashedPassword = sha.ComputeHash(objUtf8.GetBytes(password));

            //convert hashed password from byte[] to string to store as string in db
            StringBuilder hexString = new StringBuilder(hashedPassword.Length * 2);
            foreach (byte b in hashedPassword)
            {
                hexString.AppendFormat("{0:x2}", b);
            }

            return hexString.ToString();
        }

        public AuthControllerTests()
        {
            _mockContext = new Mock<ElectionDbContext>();
            _mockTokenService = new Mock<IJWTService>();
            _mockEmailSenderService = new Mock<IEmailSenderService>();
            _controller = new AuthController(_mockContext.Object, _mockTokenService.Object, _mockEmailSenderService.Object);
        }

        [Fact]
        public async Task AnyAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var data = new List<ElectionUser>
    {
        new ElectionUser { Email = "test@example.com", Password = "hashed_password" }
    }.AsQueryable();

            var mockSet = MockDbSet.CreateMockDbSet(data);
            var mockContext = new Mock<ElectionDbContext>();
            mockContext.Setup(c => c.ElectionUsers).Returns(mockSet.Object);

            // Assert mock setup
            Assert.NotNull(mockContext.Object.ElectionUsers);
            Assert.IsAssignableFrom<IQueryable<ElectionUser>>(mockContext.Object.ElectionUsers);

            // Act
            var result = await mockContext.Object.ElectionUsers.AnyAsync(user => user.Email == "test@example.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Login_ReturnsToken_ForValidCredentials()
        {
            // Arrange
            var loginViewModel = new LoginViewModel { Username = "testuser@example.com", Password = "password" };


            var users = new List<ElectionUser> {  new ElectionUser { Email = "testuser@example.com", Password = HashPassword("password") }
            }.AsQueryable();
            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);

            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);


            _mockTokenService.Setup(service => service.CreateToken(It.IsAny<RSA>(), loginViewModel.Username, _mockContext.Object))
                             .ReturnsAsync("valid_token");

            // Act
            var result = await _controller.Login(loginViewModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("valid_token", okResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_ForInvalidCredentials()
        {
            // Arrange
            var loginViewModel = new LoginViewModel { Username = "testuser@example.com", Password = "wrongpassword" };

            var users = new List<ElectionUser> {  new ElectionUser { Email = "testuser@example.com", Password = HashPassword("password") }
            }.AsQueryable();

            var mockSet = MockDbSet.CreateMockDbSet(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);

            _mockTokenService.Setup(service => service.CreateToken(It.IsAny<RSA>(), loginViewModel.Username, _mockContext.Object))
                             .ReturnsAsync("valid_token");

            // Act
            var result = await _controller.Login(loginViewModel);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsSuccessfullyAdded()
        {
            // Arrange
            var registerViewModel = new RegisterViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                PESEL = "12345678901",
                DateOfBirth = new DateTime(1990, 1, 1),
                Email = "newuser@example.com",
                PhoneNumber = "123456789",
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd",
                SelectedDistrictId = 1
            };

            var users = new List<ElectionUser>().AsQueryable();
            var mockSet = MockDbSet.CreateMockDbSet(users);

            var mockSetPeople = MockDbSet.CreateMockDbSet(new List<Person>()
            {
                new Person { Pesel = "12345678901" }
            }.AsQueryable());

            var mockSetVoters = MockDbSet.CreateMockDbSet(new List<Voter>().AsQueryable());

            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);
            _mockContext.Setup(m => m.People).Returns(mockSetPeople.Object);
            _mockContext.Setup(m => m.Voters).Returns(mockSetVoters.Object);

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
        {
            // Arrange
            var registerViewModel = new RegisterViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                PESEL = "12345678901",
                DateOfBirth = new DateTime(1990, 1, 1),
                Email = "existinguser@example.com",
                PhoneNumber = "123456789",
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd",
                SelectedDistrictId = 1
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser { Email = "existinguser@example.com" }
            }.AsQueryable();

            var mockSet = MockDbSet.CreateMockDbSet(users);
            var mockSetPeople = MockDbSet.CreateMockDbSet(new List<Person>()
            {
                new Person { Pesel = "12345678901" }
            }.AsQueryable());

            var mockSetVoters = MockDbSet.CreateMockDbSet(new List<Voter>().AsQueryable());

            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);
            _mockContext.Setup(m => m.People).Returns(mockSetPeople.Object);
            _mockContext.Setup(m => m.Voters).Returns(mockSetVoters.Object);

            // Act
            var result = await _controller.Register(registerViewModel);

            // Assert
            Assert.IsType<ConflictResult>(result);
        }

        [Fact]
        public async Task RenewToken_ReturnsNewToken_ForValidToken()
        {
            // Arrange
            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "1"),
            };

            _controller.ControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            var userInfo = new UserInfoViewModel { Username = "testuser", CurrentUserType = new UserTypeViewModel() { IdUserType = 3 } };
            var userInfoSame = new UserInfoViewModel { Username = "testuser", CurrentUserType = new UserTypeViewModel() { IdUserType = 1 } };

            _mockTokenService.Setup(service => service.RenewTokenClaims(userInfo.Username, _mockContext.Object, It.IsAny<int>()))
                             .ReturnsAsync("new_token");

            // Act
            var result = await _controller.RenewTokenClaims(userInfo);
            // Assert
            var okObjResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("new_token", okObjResult.Value);

            result = await _controller.RenewTokenClaims(userInfoSame);
            Assert.IsType<OkResult>(result);


        }

        [Fact]
        public async Task RenewToken_ReturnsNoNewToken_ForInvalidToken()
        {
            // Arrange
            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "0"),
            };

            _controller.ControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            // Arrange
            var userInfo = new UserInfoViewModel { Username = "testuser", CurrentUserType = new UserTypeViewModel() { IdUserType = 3 } };
            _mockTokenService.Setup(service => service.RenewTokenClaims(userInfo.Username, _mockContext.Object, It.IsAny<int>()))
                             .ReturnsAsync((string)null);

            // Act
            var result = await _controller.RenewTokenClaims(userInfo);

            // Assert
            var okObjResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okObjResult.Value);
        }
        [Fact]
        public async Task Verify2faOnFirstRegistration_ReturnsBadRequest_When2FAAlreadyEnabled()
        {
            // Arrange
            var userSecret = NewUserSecret;

            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "1"),
                new Claim("2FAenabled", "true"),
                new Claim("IdElectionUser", "1"),
            };

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            var verReq = new TwoFactorAuthVerifyRequest() { Code = GenerateTotpCode(userSecret), UserId = 1 };

            _controller.ControllerContext = mockControllerContext;

            // Act
            var result = await _controller.Verify2faOnFirstRegistration(verReq);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("2FA is already enabled for this user!", badRequestResult.Value);
        }

        private static string NewUserSecret { get => Base32Encoding.ToString(Guid.NewGuid().ToByteArray()).Substring(0, 16); }

        private string GenerateTotpCode(string userSecret, int timeWindow = 30)
        {
            var totp = new Totp(Base32Encoding.ToBytes(userSecret), step: timeWindow);
            return totp.ComputeTotp(DateTime.UtcNow);
        }

        [Fact]
        public async Task Verify2faOnFirstRegistration_ReturnsOk_When2FAVerified()
        {
            // Arrange
            var userSecret = NewUserSecret;
            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "1"),
                new Claim("2FAdisabled", "true"),
                new Claim("IdElectionUser", "1"),
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser { Email = "existinguser@example.com", IdElectionUser = 1, UserSecret = userSecret}
            }.AsQueryable();

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            var verReq = new TwoFactorAuthVerifyRequest() { Code = GenerateTotpCode(userSecret), UserId = 1 };

            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);

            _controller.ControllerContext = mockControllerContext;


            _mockTokenService.Setup(s => s.RenewTokenClaims(It.IsAny<string>(), It.IsAny<ElectionDbContext>(), It.IsAny<int>()))
                            .ReturnsAsync("newTokenTwoFaEnabled");

            // Act
            var result = await _controller.Verify2faOnFirstRegistration(verReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("newTokenTwoFaEnabled", okResult.Value);

            var user = users.First();
            Assert.True(user.Is2Faenabled);
        }

        [Fact]
        public async Task Verify2faOnFirstRegistration_ReturnsNotFound_WhenClaimToModelComparisonFails()
        {
            // Arrange
            var userSecret = NewUserSecret;
            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "1"),
                new Claim("2FAdisabled", "true"),
                new Claim("IdElectionUser", "1"),
            };

            var verReq = new TwoFactorAuthVerifyRequest() { Code = GenerateTotpCode(userSecret), UserId = 2 };

            _controller.ControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            // Act
            var result = await _controller.Verify2faOnFirstRegistration(verReq);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Wrong user identification compared claim to model!", notFound.Value);
        }


        [Fact]
        public async Task Verify2faOnFirstRegistration_ReturnsNotFound_WhenUserDoesNotHaveUserSecretOrExists()
        {
            // Arrange
            var userSecret = NewUserSecret;
            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "1"),
                new Claim("2FAdisabled", "true"),
                new Claim("IdElectionUser", "1"),
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser { Email = "existinguser@example.com", IdElectionUser = 1, UserSecret = null}
            }.AsQueryable();

            var usersEmpty = new List<ElectionUser>().AsQueryable();
            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            var verReq = new TwoFactorAuthVerifyRequest() { Code = GenerateTotpCode(userSecret), UserId = 1 };

            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);

            _controller.ControllerContext = mockControllerContext;

            // Act
            var result = await _controller.Verify2faOnFirstRegistration(verReq);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User with 2fa capabilities does not exists!", notFound.Value);

            mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(usersEmpty);
            result = await _controller.Verify2faOnFirstRegistration(verReq);
            notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal("User with 2fa capabilities does not exists!", notFound.Value);

        }

        [Fact]
        public async Task Verify2fa_ReturnsOk_When2FAVerified()
        {
            // Arrange
            var userSecret = NewUserSecret;
            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "1"),
                new Claim("2FAenabled", "true"),
                new Claim("IdElectionUser", "1"),
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser { Email = "existinguser@example.com", IdElectionUser = 1, UserSecret = userSecret}
            }.AsQueryable();

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            var verReq = new TwoFactorAuthVerifyRequest() { Code = GenerateTotpCode(userSecret), UserId = 1 };

            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);

            _controller.ControllerContext = mockControllerContext;


            _mockTokenService.Setup(s => s.TwoFaVeryfiedToken(It.IsAny<string>(), It.IsAny<ElectionDbContext>(), It.IsAny<int>(), It.IsAny<bool>()))
                            .ReturnsAsync("newTokenTwoFaVeryfied");

            // Act
            var result = await _controller.Verify2fa(verReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("newTokenTwoFaVeryfied", okResult.Value);
        }

        [Fact]
        public async Task Verify2fa_ReturnsUnauthorized_WhenBadCode()
        {
            // Arrange
            var userSecret = NewUserSecret;
            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "1"),
                new Claim("2FAenabled", "true"),
                new Claim("IdElectionUser", "1"),
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser { Email = "existinguser@example.com", IdElectionUser = 1, UserSecret = userSecret}
            }.AsQueryable();

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            var verReq = new TwoFactorAuthVerifyRequest() { Code = "verybadcode", UserId = 1 };

            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);

            _controller.ControllerContext = mockControllerContext;

            // Act
            var result = await _controller.Verify2fa(verReq);

            // Assert
            var unauthorizedObjResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Wrong TOTP code", unauthorizedObjResult.Value);
        }

        [Fact]
        public async Task GetAuthenticationKey_WhenUserExistsOrNot()
        {
            // Arrange
            int userId = 1;
            var mockClaims = new List<Claim>
            {
                new Claim("IdElectionUser", userId.ToString()),
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser { Email = "existinguser@example.com", IdElectionUser = userId, UserSecret = null}
            }.AsQueryable();

            _controller.ControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);


            // Act
            var result = await _controller.GetAuthenticatorKey(userId);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFound.Value);

            _mockContext.Setup(m => m.ElectionUsers.FindAsync(userId)).ReturnsAsync(users.First(u => u.IdElectionUser == userId));

            // Act
            result = await _controller.GetAuthenticatorKey(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var user = users.First();
            Assert.NotNull(user.UserSecret);
            Assert.Equal(user.UserSecret, okResult.Value);


        }

        [Fact]
        public async Task Reset2FA_ReturnOk_WhenUserExists()
        {
            // Arrange
            int userId = 1;
            var userSecret = NewUserSecret;
            var mockClaims = new List<Claim>
            {
                new Claim("IdUserType", "1"),
                new Claim("2FAdisabled", "true"),
                new Claim("IdElectionUser", userId.ToString()),
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser { Email = "existinguser@example.com", IdElectionUser = userId, UserSecret = userSecret, Is2Faenabled = false}
            }.AsQueryable();

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);


            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);
            _mockContext.Setup(m => m.ElectionUsers.FindAsync(userId)).ReturnsAsync(users.First(u => u.IdElectionUser == userId));

            _controller.ControllerContext = mockControllerContext;


            _mockTokenService.Setup(s => s.RenewTokenClaims(It.IsAny<string>(), It.IsAny<ElectionDbContext>(), It.IsAny<int>()))
                            .ReturnsAsync("newTokenTwoFaDisabled");

            // Act
            var result = await _controller.Reset2FA(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("newTokenTwoFaDisabled", okResult.Value);

            var user = users.First();
            Assert.False(user.Is2Faenabled);
            Assert.Null(user.UserSecret);
        }

        [Fact]
        public async Task ForgetPassword_WhenUserExists()
        {
            // Arrange

            var users = new List<ElectionUser>
            {
                new ElectionUser { Email = "existinguser@example.com", IdElectionUser = 1, UserSecret = null}
            }.AsQueryable();

            var forgetModel = new ForgetPasswordViewModel() { Email = "existinguser@example.com" };

            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);

            var emailSendOperation = new Mock<EmailSendOperation>();
            emailSendOperation.Setup(e => e.HasCompleted).Returns(true);

            _mockEmailSenderService.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync(emailSendOperation.Object);

            // Act
            var result = await _controller.ForgetPassword(forgetModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Password reset code sent to your email.", okResult.Value);

            var user = users.First();
            Assert.NotNull(user.UserSecret);

            // Arrange
            emailSendOperation.Setup(e => e.HasCompleted).Returns(false);

            //Act 
            result = await _controller.ForgetPassword(forgetModel);

            // Assert

            var internalServerError = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, internalServerError.StatusCode);
            Assert.Equal("Failed to send email", internalServerError.Value);

        }

        [Fact]
        public async Task ResetPassword_ReturnOk_WhenSuccess()
        {
            // Arrange
            int userId = 1;
            var userSecret = NewUserSecret;
            var newPassword = "P@ssw0rd";
            var newPasswordHashed = HashPassword(newPassword);
            var oldPassword = HashPassword("P@ssw0rd1");

            var users = new List<ElectionUser>
            {
                new ElectionUser {
                    Email = "existinguser@example.com",
                    Password = oldPassword,
                    IdElectionUser = userId,
                    UserSecret = userSecret,
                    Is2Faenabled = true
                }
            }.AsQueryable();

            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);

            var resetModel = new ResetPasswordViewModel()
            {
                Email = "existinguser@example.com",
                ResetCode = GenerateTotpCode(userSecret, timeWindow: 120),
                NewPassword = newPassword,
                NewConfirmPassword = newPassword
            };

            // Act
            var result = await _controller.ResetPassword(resetModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Password reset successfully", okResult.Value);

            var user = users.First();
            Assert.False(user.Is2Faenabled);
            Assert.Null(user.UserSecret);
            Assert.Equal(user.Password, newPasswordHashed);
        }

        [Fact]
        public async Task ResetPassword_ReturnBadRequest_WhenNotSuccess()
        {
            // Arrange
            int userId = 1;
            var userSecret = NewUserSecret;
            var newPassword = "P@ssw0rd";
            var newPasswordHashed = HashPassword(newPassword);
            var oldPassword = HashPassword("P@ssw0rd1");

            var users = new List<ElectionUser>
            {
                new ElectionUser {
                    Email = "existinguser@example.com",
                    Password = oldPassword,
                    IdElectionUser = userId,
                    UserSecret = userSecret,
                    Is2Faenabled = true
                }
            }.AsQueryable();

            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);

            var resetModel = new ResetPasswordViewModel()
            {
                Email = "existinguser@example.com",
                ResetCode = GenerateTotpCode(userSecret, timeWindow: 60),
                NewPassword = newPassword,
                NewConfirmPassword = newPassword
            };

            var user = users.First();
            user.Password = newPasswordHashed;

            // Act
            var result = await _controller.ResetPassword(resetModel);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Password cannot be the same as previous one", badRequest.Value);


            //Arrange
            resetModel.ResetCode = "badCode";
            //Act
            result = await _controller.ResetPassword(resetModel);
            //Assert
            badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid reset code", badRequest.Value);

            //Arrange
            user.UserSecret = null;
            //Act
            result = await _controller.ResetPassword(resetModel);
            //Assert
            badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User has not requested password reset code", badRequest.Value);

        }

        [Fact]
        public async Task GetCurrentVoterDistrictId_WhenVoterExists()
        {
            // Arrange
            int userId = 1;
            int districtId = 1; 
            var mockClaims = new List<Claim>
            {
                new Claim("IdElectionUser", userId.ToString()),
            };

            var voters = new List<Voter>
            {
                new Voter {  IdElectionUser = userId, IdDistrict = districtId}
            }.AsQueryable();

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);


            var mockSet = MockDbSet.CreateMockDbSet<Voter>(voters);
            _mockContext.Setup(m => m.Voters).Returns(mockSet.Object);

            _controller.ControllerContext = mockControllerContext;

            // Act
            var result = await _controller.GetCurrentVoterDistrictId();

            // Assert
            var actionResult = Assert.IsType<ActionResult<int>>(result);
            Assert.Equal(districtId, actionResult.Value);
        }

        [Fact]
        public async Task GetCurrentUserDistrictId_WhenVoterExists()
        {
            // Arrange
            int userId = 1;
            int districtId = 1;
            var mockClaims = new List<Claim>
            {
                new Claim("IdElectionUser", userId.ToString()),
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser {  IdElectionUser = userId, IdDistrict = districtId}
            }.AsQueryable();

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);


            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);
            _mockContext.Setup(m => m.ElectionUsers.FindAsync(userId)).ReturnsAsync(users.First(u => u.IdElectionUser == userId));

            _controller.ControllerContext = mockControllerContext;

            // Act
            var result = await _controller.GetCurrentUserDistrictId();

            // Assert
            var actionResult = Assert.IsType<ActionResult<int>>(result);
            Assert.Equal(districtId, actionResult.Value);
        }

        [Fact]
        public async Task GetCurrentUserVoterId_WhenVoterExists()
        {
            // Arrange
            int userId = 1;
            int voterId = 1;
            var mockClaims = new List<Claim>
            {
                new Claim("IdElectionUser", userId.ToString()),
            };

            var voters = new List<Voter>
            {
                new Voter {  IdElectionUser = userId, IdVoter = voterId}
            }.AsQueryable();

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);


            var mockSet = MockDbSet.CreateMockDbSet<Voter>(voters);
            _mockContext.Setup(m => m.Voters).Returns(mockSet.Object);

            _controller.ControllerContext = mockControllerContext;

            // Act
            var result = await _controller.GetCurrentUserVoterId();

            // Assert
            var actionResult = Assert.IsType<ActionResult<int>>(result);
            Assert.Equal(voterId, actionResult.Value);
        }

        [Fact]
        public async Task GetCurrentUser2faStatus_WhenVoterExists()
        {
            // Arrange
            int userId = 1;
            var mockClaims = new List<Claim>
            {
                new Claim("IdElectionUser", userId.ToString()),
                new Claim(ClaimTypes.Role, "2FAveryfiedUser")
            };

            var users = new List<ElectionUser>
            {
                new ElectionUser {  IdElectionUser = userId}
            }.AsQueryable();

            var mockControllerContext = MockControllerContext.WithUser("testuser", mockClaims);


            var mockSet = MockDbSet.CreateMockDbSet<ElectionUser>(users);
            _mockContext.Setup(m => m.ElectionUsers).Returns(mockSet.Object);
            _mockContext.Setup(m => m.ElectionUsers.FindAsync(userId)).ReturnsAsync(users.First(u => u.IdElectionUser == userId));

            _controller.ControllerContext = mockControllerContext;  

            // Act
            var result = await _controller.GetCurrentUser2faStatus();

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            Assert.True(actionResult.Value);

            // Arrange 
            mockClaims.RemoveAt(1);
            _controller.ControllerContext = MockControllerContext.WithUser("testuser", mockClaims);

            // Act
            result = await _controller.GetCurrentUser2faStatus();

            // Assert
            actionResult = Assert.IsType<ActionResult<bool>>(result);
            Assert.False(actionResult.Value);
        }
    }
}