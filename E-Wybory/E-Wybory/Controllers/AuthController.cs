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


namespace E_Wybory.Controllers
{
    public static class JWTMethods
    {
        public const int JWT_TOKEN_VALIDATION_MINS = 10;
        public static void CreateRSAPrivateKey()
        {
            var rsaKey = RSA.Create();
            var privateKey = rsaKey.ExportRSAPrivateKey();
            File.WriteAllBytes("key", privateKey);
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
            new Claim("name", username)

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
                CreateRSAPrivateKey();
                var rsaKey = RSA.Create();
                rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
                return createToken(rsaKey, email);
            }
            return null;
        }

        public static void Register(string name, string surname, string PESEL, DateTime birthdate, string email, 
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

            //add new People's record
            var person = new Person();

            person.Name = name;
            person.Surname = surname;
            person.Pesel = PESEL;
            person.BirthDate = birthdate;

            context.People.Add(person);
            context.SaveChanges();
            var newPersonId = person.IdPerson; //save to use in user

            //add new user
            var user = new ElectionUser();
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            user.Password = hexString.ToString();
            user.IdPerson = newPersonId;
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
        }
    }

    public class RegistrationRequest
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string PESEL { get; set; }
        public DateTime birthdate { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string password { get; set; }
        public int idDistrict { get; set; }
    }

    public class AuthenticationRequest
    {
        public string email { set; get; }
        public string password { set; get; }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ElectionDbContext context;
        public AuthController(ElectionDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [Route("api/login")]
        [AllowAnonymous]
        public IActionResult Login([FromForm] AuthenticationRequest request)
        {
            var authResult = JWTMethods.Authenticate(request.email, request.password, context);
            if (authResult == null)
                return Unauthorized();

            return Ok(authResult);
        }

        [HttpPost]
        [Route("api/register")]
        [AllowAnonymous]
        public IActionResult Register([FromForm] RegistrationRequest request)
        {
            JWTMethods.Register(request.name, request.surname, request.PESEL, request.birthdate, request.email,
            request.phoneNumber, request.password, request.idDistrict, context);

            return Ok();
        }
    }
}
