using E_Wybory.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using E_Wybory.Infrastructure;
using E_Wybory.Infrastructure.DbContext;
using System.Diagnostics;
using System.Text;
using System.Collections;
using E_Wybory.Domain.Entities;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Xml.Linq;
using E_Wybory.Client.ViewModels;
using E_Wybory.Services;
using System.IO;
using Microsoft.EntityFrameworkCore;
using E_Wybory.Application.DTOs;
using E_Wybory.Application.Wrappers;
using OtpNet;
using Microsoft.AspNetCore.Identity.Data;
using System.Net.Mail;
using System.Collections.Generic;
using Azure.Communication.Email;
using System.Net;
using E_Wybory.Services.Interfaces;
using E_Wybory.Client.Validators;


namespace E_Wybory.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ElectionDbContext _context;
        private readonly IJWTService _tokenService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly ILogger<AuthController> _logger;
        private IPdfGeneratorService pdfGeneratorService = new PdfGenerateService();
        private static readonly ElectionPasswordPolicyAttribute _policyAttribute = new ElectionPasswordPolicyAttribute();

        public AuthController(ElectionDbContext context, IJWTService tokenService, IEmailSenderService emailSenderService, ILogger<AuthController> logger)
        {
            this._context = context;
            this._tokenService = tokenService;
            this._emailSenderService = emailSenderService;
            _logger = logger;   
        }


        private bool CheckPasswordyPolicy(string password)
        {
            return _policyAttribute.IsValid(password);
        }


        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
            _logger.LogInformation("Login attempt with username: {0}", request.Username);
            if (request.Username == String.Empty || request.Password == String.Empty)
                return BadRequest("Not entered data to all required fields");

            var authResult = await AuthenticateUser(request.Username, request.Password);
            if (authResult == null)
                return Unauthorized();
            else
                return Ok(authResult);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel request)
        {
            if (!CheckPasswordyPolicy(request.Password)) return BadRequest("Password does not meet the policy requirements");
            if (!ModelState.IsValid) return BadRequest("Not entered data to all required fields");


            bool registerResult = await RegisterUser(request.FirstName, request.LastName, request.PESEL, request.DateOfBirth, request.Email,
            request.PhoneNumber, request.Password, request.SelectedDistrictId);
            if (registerResult)
                return Ok();
            else
                return Conflict();
        }

        [HttpPost]
        [Route("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();

            return string.IsNullOrEmpty(authorizationHeader) ? Ok() : BadRequest("Auth token was not cleared properly");

        }

        [HttpPost]
        [Route("renew-token")]
        [Authorize]
        public async Task<IActionResult> RenewTokenClaims([FromBody] UserInfoViewModel userInfo)
        {
            string? currentUserTypeId = User.Claims.FirstOrDefault(c => c.Type.Equals("IdUserType"))?.Value;
            
            if(string.IsNullOrEmpty(currentUserTypeId) || Convert.ToInt32(currentUserTypeId) == userInfo.CurrentUserType.IdUserType)
            {
                return Ok();
            }

            string? username = User.Claims.FirstOrDefault(c => c.Type.Equals("name"))?.Value;
            
            if (username is null || !username.Equals(userInfo.Username)) return Unauthorized("Invalid token structure with model compatiblity");

            var newToken = await _tokenService.RenewTokenClaims(username, _context, userInfo.CurrentUserType.IdUserType);

            return  Ok(newToken);

        }

        [HttpPost]
        [Route("verify-2fa-first")]
        [Authorize]
        public async Task<IActionResult> Verify2faOnFirstRegistration([FromBody] TwoFactorAuthVerifyRequest verReq)
        {
            UserWrapper user = new UserWrapper(User);

            if (user.TwoFAenabled) return BadRequest("2FA is already enabled for this user!");

            var actionResult = await VerifyTwoFactorTokenWithUser(user, verReq, shouldEnable: true);

            if (actionResult is OkObjectResult okResult)
            {
                okResult.Value = await _tokenService.RenewTokenClaims(user.Username!, _context, user.IdUserType);   
            }

            return actionResult;
        }


        private async Task<IActionResult> VerifyTwoFactorTokenWithUser(UserWrapper user, TwoFactorAuthVerifyRequest verReq, bool shouldEnable = false)
        {
            if (user.Id == 0 || verReq.UserId == 0 || verReq.UserId != user.Id) return NotFound("Wrong user identification compared claim to model!");

            var electionUser = await _context.ElectionUsers.FirstOrDefaultAsync(e => e.IdElectionUser == user.Id);

            if (electionUser is null || string.IsNullOrEmpty(electionUser.UserSecret)) return NotFound("User with 2fa capabilities does not exists!");

            bool verResult = VerifyTotpCode(electionUser.UserSecret, verReq.Code);

            if (verResult && shouldEnable)
            {
                electionUser.Is2Faenabled = true;
                _context.ElectionUsers.Update(electionUser);
                await _context.SaveChangesAsync();
            }

            return verResult ? Ok("") : BadRequest("Wrong TOTP code");
        }

        [HttpPost]
        [Route("verify-2fa")]
        [Authorize(Policy = "2FAenabled")]
        public async Task<IActionResult> Verify2fa([FromBody] TwoFactorAuthVerifyRequest verReq)
        {
            UserWrapper user = new UserWrapper(User);

            var actionResult = await VerifyTwoFactorTokenWithUser(user, verReq, shouldEnable: false);

            if (actionResult is OkObjectResult okResult)
            {
                okResult.Value = await _tokenService.TwoFaVeryfiedToken(user.Username!, _context, user.IdUserType, is2FAveryfied: true);
            }

            return actionResult;


        }

        [HttpGet("count-rec-codes/{userId}")]
        [Authorize]
        public async Task<IActionResult> CountRecCodes(int userId)
        {
            UserWrapper user = new(User);
            if (userId == 0 || user.Id == 0 || user.Id != userId) return NotFound("Wrong user identification compared claim to model!");
                        
            return Ok(new CountResponse { Count = 0});
        }

       //????
        private const int maxRecoveryCodes = 6;

        [HttpGet("gen-rec-codes/{userId}")]
        [Authorize]
        public async Task<IActionResult> GenerateNewTwoFactorRecoveryCodes(int userId)
        {
            var codes = new List<string>();
            for (int i = 0; i < maxRecoveryCodes; i++)
            {
                codes.Add(Guid.NewGuid().ToString().Substring(0, 8));
            }
            return Ok(codes);
        }

        [HttpGet("get-auth-key/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetAuthenticatorKey(int userId)
        {
            UserWrapper user = new(User);
            if (userId == 0 || user.Id == 0 || user.Id != userId) return NotFound("Wrong user identification compared claim to model!");

            var electionUser = await _context.ElectionUsers.FindAsync(userId);
            if (electionUser == null)
            {
                return NotFound("User not found");
            }

            if (string.IsNullOrEmpty(electionUser.UserSecret))
            {
                electionUser.UserSecret = Base32Encoding.ToString(Guid.NewGuid().ToByteArray()).Substring(0, 16);
                _context.ElectionUsers.Update(electionUser);
                await _context.SaveChangesAsync();
            }

            return Ok(electionUser.UserSecret);
        }

        [HttpPost("reset-2fa/{userId}")]
        [Authorize(Policy ="2FAenabled")]
        public async Task<IActionResult> Reset2FA(int userId)
        {
            UserWrapper user = new(User);
            if (userId == 0 || user.Id == 0 || user.Id != userId) return NotFound("Wrong user identification compared claim to model!");

            var electionUser = await _context.ElectionUsers.FindAsync(userId);
            if (electionUser == null)
            {
                return NotFound("User not found");
            }

            electionUser.UserSecret = null;
            electionUser.Is2Faenabled = false;

            _context.ElectionUsers.Update(electionUser);
            await _context.SaveChangesAsync();

            string newToken = await _tokenService.RenewTokenClaims(user.Username!, _context, user.IdUserType);

            return Ok(newToken);
        }

        private bool VerifyTotpCode(string userSecret, string code, int timeWindow = 30)
        {
            try
            {
                byte[] secretBytes = Base32Encoding.ToBytes(userSecret);

                Totp totp = new Totp(secretBytes, step: timeWindow, mode: OtpHashMode.Sha1);

                bool isCodeValid = totp.VerifyTotp(code, out long timeStepMatched, window: new VerificationWindow(previous: 1, future: 1));

                return isCodeValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying TOTP code: {ex.Message}");
                return false;
            }
        }

        [HttpPost]
        [Route("forget-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword([FromBody]  ForgetPasswordViewModel forgetPassword)
        {
            if(!ModelState.IsValid) return BadRequest("Invalid model state");

            var user = await _context.ElectionUsers.FirstOrDefaultAsync(u => u.Email == forgetPassword.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if(user.UserSecret == null)
            {
                user.UserSecret = Base32Encoding.ToString(Guid.NewGuid().ToByteArray()).Substring(0, 16);
                _context.ElectionUsers.Update(user);
                await _context.SaveChangesAsync();
            }

            var totpCode = GenerateTotpCode(user.UserSecret, timeWindow: 120);
            var emailMessage = $"Twój jednorazowy kod do resetowania hasła to: {totpCode}. Jest ważny przez dwie minuty.";

            var emailOperationResult =  await _emailSenderService.SendEmailAsync(user.Email, "E-wybory: Reset hasła", emailMessage);


            IActionResult result = emailOperationResult is not null && emailOperationResult.HasCompleted ? 
                Ok("Password reset code sent to your email.") : StatusCode(500, "Failed to send email");

            return result;
        }

        [HttpPost]
        [Route("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel request)
        {
            if (!CheckPasswordyPolicy(request.NewPassword)) return BadRequest("Password does not meet the policy requirements");
            if (!ModelState.IsValid) return BadRequest("Invalid model state");

            var user = await _context.ElectionUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.UserSecret == null)
            {
                return BadRequest("User has not requested password reset code");
            }

            var isCodeValid = VerifyTotpCode(user.UserSecret, request.ResetCode, timeWindow: 120);
            if (!isCodeValid)
            {
                return BadRequest("Invalid reset code");
            }

            string hashedNewPassword = HashPassword(request.NewPassword);

            if (hashedNewPassword == user.Password)
            {
                return BadRequest("Password cannot be the same as previous one");
            }

            user.Password = hashedNewPassword;

            user.UserSecret = null;
            user.Is2Faenabled = false;

            _context.ElectionUsers.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Password reset successfully");
        }


        private string GenerateTotpCode(string userSecret, int timeWindow)
        {
            var key = Base32Encoding.ToBytes(userSecret);
            var totp = new Totp(key, step: timeWindow);
            return totp.ComputeTotp(DateTime.UtcNow);
        }        

        private async Task<string> AuthenticateUser(string email, string password)
        {
            string hexString = HashPassword(password);

            if (await _context.ElectionUsers.AnyAsync(user => user.Email == email) &&
                await _context.ElectionUsers
                .Where(user => user.Email == email)
                .Select(user => user.Password)
                .FirstOrDefaultAsync() == hexString
                )
            {
                var rsaKey = RSA.Create();
                rsaKey.ImportRSAPrivateKey(System.IO.File.ReadAllBytes("key"), out _);
                return await _tokenService.CreateToken(rsaKey, email, _context);
            }
            return null;
        }

        private string HashPassword(string password)
        {
            SHA256 sha = SHA256.Create();
            UTF8Encoding objUtf8 = new UTF8Encoding();
            byte[] hashedPassword = sha.ComputeHash(objUtf8.GetBytes(password));

            StringBuilder hexString = new StringBuilder(hashedPassword.Length * 2);
            foreach (byte b in hashedPassword)
            {
                hexString.AppendFormat("{0:x2}", b);
            }

            return hexString.ToString();
        }

        private async Task<bool> RegisterUser(string name, string surname, string PESEL, DateTime birthdate, string email,
            string phoneNumber, string password, int idDistrict)
        {
            

            if (await _context.ElectionUsers.AnyAsync(user => user.Email == email))
            {
                return false;
            }
            string hashedPassword = HashPassword(password);
            var newPersonId = 0;

            if (!await _context.People.AnyAsync(person => person.Pesel == PESEL))
            {

                //add new People's record
                var person = new Person();

                person.Name = name;
                person.Surname = surname;
                person.Pesel = PESEL;
                person.BirthDate = birthdate;

                await _context.People.AddAsync(person);
                await _context.SaveChangesAsync();
                newPersonId = person.IdPerson; //save to use in user
            }
            else
            {
                var existingPerson = await _context.People.Where(person => person.Pesel == PESEL).FirstOrDefaultAsync();
                newPersonId = existingPerson.IdPerson;
            }

            //add new user
            var user = new ElectionUser();
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            user.Password = hashedPassword;
            user.IdPerson = newPersonId;

            user.IdDistrict = idDistrict;

            await _context.ElectionUsers.AddAsync(user);
            await _context.SaveChangesAsync();

            var userId = user.IdElectionUser; //save to use in voter
            //add new voter(every user(person) is voter too)
            var voter = new Voter();
            voter.IdDistrict = null;
            voter.IdElectionUser = userId;

            await _context.Voters.AddAsync(voter);
            await _context.SaveChangesAsync();

            return true;
        }

        [HttpGet("currentUserDistrict")]
        [Authorize] //[Authorize(Roles = "Komisja wyborcza,Administratorzy")]
        public async Task<ActionResult<int>> GetCurrentUserDistrictId()
        {

            UserWrapper user = new(User);
            var electionUser = await _context.ElectionUsers.FindAsync(user.Id);
            if(electionUser == null)
            {
                return NotFound("Not found user set to this id");
            }

            return electionUser.IdDistrict;
        }

        [HttpGet("currentVoterDistrict")]
        [Authorize]
        public async Task<ActionResult<int>> GetCurrentVoterDistrictId()
        {

            UserWrapper user = new(User);
            var voter = await _context.Voters.Where(voter => voter.IdElectionUser == user.Id).FirstOrDefaultAsync();
            if (voter == null)
            {
                return NotFound("Not found user set to this id");
            }

            return voter.IdDistrict;
        }


        [HttpGet("currentUserVoterId")]
        [Authorize]
        public async Task<ActionResult<int>> GetCurrentUserVoterId()
        {

            UserWrapper user = new(User);
            var electionUser = await _context.Voters.Where(voter => voter.IdElectionUser == user.Id).FirstOrDefaultAsync();
            if (electionUser == null)
            {
                return NotFound("Not found user set to this id");
            }

            return electionUser.IdVoter;
        }

        [HttpGet("currentUser2fa")]
        [Authorize]
        public async Task<ActionResult<bool>> GetCurrentUser2faStatus()
        {
            UserWrapper user = new(User);
            var electionUser = await _context.ElectionUsers.FindAsync(user.Id);
            if (electionUser == null)
            {
                return NotFound("Not found user set to this id");
            }

            return user.TwoFAveryfied;

        }

        [HttpGet("voteConfirmation")]
        public async Task<ActionResult<bool>> CreatePdfOfVotingConfirmation()
        {
            UserWrapper user = new(User);
            var electionUser = await _context.ElectionUsers.FindAsync(user.Id);
            if (electionUser == null)
            {
                return NotFound("Not found user set to this id");
            }
            var person = await _context.People.FindAsync(electionUser.IdPerson);

            try
            {
                var pdfPath = await pdfGeneratorService.GeneratePdfWithImage_Syncfusion("Zaświadczenie o głosowaniu", $"Potwierdzamy oddanie " +
                    $"głosu przez użytkownika " + $"{person.Name} {person.Surname} posługującego się numerem PESEL: {person.Pesel} \n " +
                    $"Pozdrawiamy, \n" +
                    $"Twórcy aplikacji E-Wybory", $"generatedDocument_{user.Id}.pdf");

                var emailOperationResult = await _emailSenderService.SendEmailWithPdfAttachmentAsync(electionUser.Email, "E-wybory: Potwierdzenie głosowania", "Dzień dobry,\n " +
                                                                    "W załączeniu przesyłamy zaświadczenie potwierdzające uczestnictwo w wyborach na platformie E-Wybory. \n " +
                                                                    "Pozdrawiamy,\nTwórcy aplikacji E-Wybory", pdfPath);

                if (System.IO.File.Exists(pdfPath))
                {
                    System.IO.File.Delete(pdfPath);
                }
                IActionResult result = emailOperationResult is not null && emailOperationResult.HasCompleted ?
                Ok("Pdf file sent in email correctly.") : StatusCode(500, "Failed to send email");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return Ok();
        }

    }
}
