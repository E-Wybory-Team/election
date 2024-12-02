using System.Threading.Tasks;
using E_Wybory.Controllers;
using E_Wybory.Infrastructure.DbContext;
using E_Wybory.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using E_Wybory.Services;
using E_Wybory.Client.ViewModels;
using E_Wybory.Application.DTOs;
using E_Wybory.Services.Interfaces;
using System.Security.Cryptography;
using E_Wybory.Test.Server.Utils;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Diagnostics;

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
        public async Task Register_AddsUserToDatabase()
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
            //_controller.ControllerContext = null;
            // Assert
            var okObjResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("new_token", okObjResult.Value);

            result = await _controller.RenewTokenClaims(userInfoSame);
            Assert.IsType<OkResult>(result);

            //_controller.ControllerContext = MockControllerContext.DefaultContext();
            Debug.WriteLine(_controller.ControllerContext);

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
    }
}