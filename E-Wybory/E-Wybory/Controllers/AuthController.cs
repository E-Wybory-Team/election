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


namespace E_Wybory.Controllers
{
    public static class AuthMethods
    {
        public static string Authenticate(string email, string password, ElectionDbContext context)
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

            if (context.ElectionUsers.Any(user => user.Email == email) &&
                context.ElectionUsers
                .Where(user => user.Email == email)
                .Select(user => user.Password)
                .FirstOrDefault() == hexString.ToString()
                )
            {
                //CreateRSAPrivateKey();
                var rsaKey = RSA.Create();
                rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
                return TokenService.createToken(rsaKey, email);
            }
            return null;
        }

        public static bool Register(string name, string surname, string PESEL, DateTime birthdate, string email, 
            string phoneNumber, string password, int idDistrict, ElectionDbContext context)
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

            if (context.People.Any(person => person.Pesel == PESEL)
                || context.ElectionUsers.Any(user => user.Email == email))
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
            context.People.Add(person);
            context.SaveChanges();
            var newPersonId = person.IdPerson; //save to use in user

            //add new user
            var user = new ElectionUser();
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            user.Password = hexString.ToString();
            user.IdPerson = newPersonId;

            //idDistrict = 1;
            user.IdDistrict = idDistrict;

            context.ElectionUsers.Add(user);
            context.SaveChanges();

            var userId = user.IdElectionUser; //save to use in voter
            //add new voter(every user(person) is voter too)
            var voter = new Voter();
            voter.IdDistrict = idDistrict;
            voter.IdElectionUser = userId;

            context.Voters.Add(voter);
            context.SaveChanges();

            return true;
        }
    }

    
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ElectionDbContext context;
        public AuthController(ElectionDbContext context)
        {
            this.context = context;
        }


        //For now for this endpoints use viewmodels, maybe the better options are DTO
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginViewModel request)
        {
            if (request.Username == String.Empty || request.Password == String.Empty)
                return BadRequest("Not entered data to all required fields");

            var authResult = AuthMethods.Authenticate(request.Username, request.Password, context);
            if (authResult == null)
                return Unauthorized();
            else
                return Ok(authResult);
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromBody] RegisterViewModel request)
        {
            //if(ModelState.IsValid)
            if (request.FirstName == String.Empty || request.LastName == String.Empty || request.PESEL == String.Empty
                || request.DateOfBirth == DateTime.MinValue || request.Email == String.Empty
                || request.PhoneNumber == String.Empty || request.Password == String.Empty
                || request.SelectedDistrictId == 0)

                return BadRequest("Not entered data to all required fields"); 

            bool registerResult = AuthMethods.Register(request.FirstName, request.LastName, request.PESEL, request.DateOfBirth, request.Email,
            request.PhoneNumber, request.Password, request.SelectedDistrictId, context);

            if (registerResult)
                return Ok();
            else
                return Conflict();
        }
    }
}
