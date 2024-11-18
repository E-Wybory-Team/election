﻿using E_Wybory.ExtensionMethods;
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


namespace E_Wybory.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ElectionDbContext _context;
        private readonly IJWTService _tokenService;
        public AuthController(ElectionDbContext context, IJWTService tokenService)
        {
            this._context = context;
            this._tokenService = tokenService;
        }


        //For now for this endpoints use viewmodels, maybe the better options are DTO
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel request)
        {
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
            //if(ModelState.IsValid)
            if (request.FirstName == String.Empty || request.LastName == String.Empty || request.PESEL == String.Empty
                || request.DateOfBirth == DateTime.MinValue || request.Email == String.Empty
                || request.PhoneNumber == String.Empty || request.Password == String.Empty
                || request.SelectedDistrictId == 0)

                return BadRequest("Not entered data to all required fields");

            bool registerResult = await RegisterUser(request.FirstName, request.LastName, request.PESEL, request.DateOfBirth, request.Email,
            request.PhoneNumber, request.Password, request.SelectedDistrictId);
            if (registerResult)
                return Ok();
            else
                return Conflict();
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            //Now just checking if header is empty, maybe flag in database?
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
            //int idElectionUser = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type.Equals("IdElectionUser"))?.Value);
            
            if (username is null || !username.Equals(userInfo.Username)) return Unauthorized("Invalid token structure with model compatiblity");
            //if(idElectionUser == 0 || idElectionUser != userInfo.)

            var newToken = await _tokenService.RenewTokenClaims(username, _context, userInfo.CurrentUserType.IdUserType);

            return  Ok(newToken);

        }

        [HttpPost]
        [Route("verify-2fa-first")]
        [Authorize]
        public async Task<IActionResult> Verify2faOnFirstRegistration([FromBody] TwoFactorAuthVerifyRequest verReq)
        {
            UserWrapper user = new UserWrapper(User);

            if(user.TwoFAenabled) return BadRequest("2FA is already enabled for this user!");

            if (user.Id == 0 || verReq.UserId == 0 || verReq.UserId != user.Id) return NotFound("Wrong user identification compared claim to model!");

            var electionUser = await _context.ElectionUsers.FirstOrDefaultAsync(e => e.IdElectionUser == user.Id);

            if (electionUser is null || string.IsNullOrEmpty(electionUser.UserSecret)) return NotFound("User with 2fa capabilities does not exists!");

            bool verResult = VerifyTotpCode(electionUser.UserSecret, verReq.Code);
            
            return verResult ? Ok() : Unauthorized("Wrong TOTP code");

            //usersecret + temp jako tymczasowe? , tylko z jakich znaków składa się guid?
            //osobny endpoint na pierwszą werysikację, a potem inn na podawanie weryfikowanie samego kodu po właczeniu 2fa, żeby claimy tam pakować 


        }

        [HttpPost]
        [Route("verify-2fa")]
        [Authorize]
        public async Task<IActionResult> Verify2fa([FromBody] TwoFactorAuthVerifyRequest verReq)
        {
            UserWrapper user = new UserWrapper(User);

            if (user.Id == 0 || verReq.UserId == 0 || verReq.UserId != user.Id) return NotFound("Wrong user identification compared claim to model!");

            var electionUser = await _context.ElectionUsers.FirstOrDefaultAsync(e => e.IdElectionUser == user.Id);

            if (electionUser is null || string.IsNullOrEmpty(electionUser.UserSecret)) return NotFound("User with 2fa capabilities does not exists!");

            bool verResult = VerifyTotpCode(electionUser.UserSecret, verReq.Code);

            return verResult ? Ok() : Unauthorized("Wrong TOTP code");

            //usersecret + temp jako tymczasowe? , tylko z jakich znaków składa się guid?
            //osobny endpoint na pierwszą werysikację, a potem inn na podawanie weryfikowanie samego kodu po właczeniu 2fa, żeby claimy tam pakować 


        }

        [HttpGet("count-rec-codes/{userId}")]
        public async Task<IActionResult> CountRecCodes(int userId)
        {
            UserWrapper user = new(User);
            if (userId == 0 || user.Id == 0 || user.Id != userId) return NotFound("Wrong user identification compared claim to model!");
            
            //Do sth with recovery codes here
            
            return Ok(new CountResponse { Count = 0});
        }

        [HttpPost()]
        [Route("enable-2fa")]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactorAuth([FromBody] TwoFactorEnabledRequest enabledRequest)
        {
            UserWrapper user = new(User);
            if (enabledRequest.UserId == 0 || user.Id == 0 || user.Id != enabledRequest.UserId) return NotFound("Wrong user identification compared claim to model!");

            var electionUser = await _context.ElectionUsers.FirstOrDefaultAsync(e => e.IdElectionUser == user.Id);

            if (electionUser is null || string.IsNullOrEmpty(electionUser.UserSecret)) return NotFound("User with UserSecret does not exists!");

            electionUser.Is2Faenabled = enabledRequest.IsEnabled;

            //tutaj pakujesz claimy które są 2fAEnabled 
            _context.ElectionUsers.Update(electionUser);
            await _context.SaveChangesAsync();  

            return  Ok();


        }

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
            //userRecoveryCodes[userId] = codes;
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

        [HttpPost("reset-auth-key/{userId}")]
        [Authorize]
        public async Task<IActionResult> ResetAuthenticatorKey(int userId)
        {
            UserWrapper user = new(User);
            if (userId == 0 || user.Id == 0 || user.Id != userId) return NotFound("Wrong user identification compared claim to model!");

            var electionUser = await _context.ElectionUsers.FindAsync(userId);
            if (electionUser == null)
            {
                return NotFound("User not found");
            }

            electionUser.UserSecret = Base32Encoding.ToString(Guid.NewGuid().ToByteArray()).Substring(0, 16);
            _context.ElectionUsers.Update(electionUser);
            await _context.SaveChangesAsync();

            return Ok(electionUser.UserSecret);
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

        //private string ConvertToBase32(byte[] data)
        //{
        //    const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        //    StringBuilder result = new StringBuilder((data.Length + 4) / 5 * 8);

        //    for (int i = 0; i < data.Length; i += 5)
        //    {
        //        int buffer = data[i] << 24;
        //        if (i + 1 < data.Length) buffer |= data[i + 1] << 16;
        //        if (i + 2 < data.Length) buffer |= data[i + 2] << 8;
        //        if (i + 3 < data.Length) buffer |= data[i + 3];
        //        if (i + 4 < data.Length) buffer |= data[i + 4] >> 8;

        //        for (int bitOffset = 35; bitOffset >= 0; bitOffset -= 5)
        //        {
        //            result.Append(base32Chars[(buffer >> bitOffset) & 0x1F]);
        //        }
        //    }

        //    return result.ToString();
        //}

        private async Task<string> AuthenticateUser(string email, string password)
        {
            SHA256 sha = SHA256.Create();
            UTF8Encoding objUtf8 = new UTF8Encoding();
            byte[] hashedPassword = sha.ComputeHash(objUtf8.GetBytes(password));

            //convert hashed password from byte[] to string to compare with db's passsword
            StringBuilder hexString = new StringBuilder(hashedPassword.Length * 2);
            foreach (byte b in hashedPassword)
            {
                hexString.AppendFormat("{0:x2}", b);
            }

            if (await _context.ElectionUsers.AnyAsync(user => user.Email == email) &&
                await _context.ElectionUsers
                .Where(user => user.Email == email)
                .Select(user => user.Password)
                .FirstOrDefaultAsync() == hexString.ToString()
                )
            {
                //CreateRSAPrivateKey();
                var rsaKey = RSA.Create();
                rsaKey.ImportRSAPrivateKey(System.IO.File.ReadAllBytes("key"), out _);
                return await _tokenService.CreateToken(rsaKey, email, _context);
            }
            return null;
        }

        private async Task<bool> RegisterUser(string name, string surname, string PESEL, DateTime birthdate, string email,
            string phoneNumber, string password, int idDistrict)
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


            if (await _context.People.AnyAsync(person => person.Pesel == PESEL)
                || await _context.ElectionUsers.AnyAsync(user => user.Email == email))
            {
                return false;
            }

            //add new People's record
            var person = new Person();

            person.Name = name;
            person.Surname = surname;
            person.Pesel = PESEL;
            person.BirthDate = birthdate;

            //Console.Write(idDistrict);
            await _context.People.AddAsync(person);
            await _context.SaveChangesAsync();
            var newPersonId = person.IdPerson; //save to use in user

            //add new user
            var user = new ElectionUser();
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            user.Password = hexString.ToString();
            user.IdPerson = newPersonId;

            //idDistrict = 1;
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
       

        
        
    }
}
