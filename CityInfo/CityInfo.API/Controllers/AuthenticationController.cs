using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CityInfo.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // we won't use this outside of the class, so we can scope it to this namespace
        public class AuthenticationRequestBody
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }

        public class CityInfoUser
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }

            public CityInfoUser(int userId, string userName, string firstName, string lastName, string city)
            {
                UserId = userId;
                UserName = userName;
                FirstName = firstName;
                LastName = lastName;
                City = city;
            }
        }

        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        // will return a string which is the token
        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            // Step 1: validate the username/password
            var user = ValidateUserCredentials(authenticationRequestBody.UserName, authenticationRequestBody.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            // tokens are signed, so we need a key to sign it.  Keys are created from secrets
            // Step 2: create a token
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Authentication:SecretForKey"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // token can contain information on who the user is, key/value pairs called claims
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.UserId.ToString()));   // sub is the unique identifier
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));
            claimsForToken.Add(new Claim("city", user.City));

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,    // start of token validitiy
                DateTime.UtcNow.AddHours(1),  // when token becomes invalid
                signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return Ok(tokenToReturn);
        }

        private CityInfoUser ValidateUserCredentials(string? userName, string? password)
        {
            // we don't have a user DB or table. If you have, check the passed-through
            // username/password aganist what's stored in the database.
            //
            // For demo purposes, we assume the credentials are valid

            // return a new CityInfoUser (values would normally come from your user DB/table)
            return new CityInfoUser(
                1,
                userName ?? "",
                "Kevin",
                "Dockx",
                "Antwerp");
        }
    }
}
