using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web.Helpers;
using virtual_store.Models;


namespace virtual_store.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {


        

        private readonly VirtualStoreContext db;
        private readonly IConfiguration _config;
        public AuthController(VirtualStoreContext _db, IConfiguration config)
        {
            _config = config;
            db = _db;
        }

        public static DateTime PST()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Define the Pakistan Standard Time zone (use "Asia/Karachi" as an alternative ID)
            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");

            // Convert the UTC time to Pakistan Standard Time
            DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, pakistanTimeZone);

            // Assign it to your desired variable
            var tds = pakistanTime;
            return tds;
        }
        [HttpPost("login")]

        public IActionResult Login([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest(new { message = "User data is required" });
            }

            var check_user_authentication = db.Users
                .Where(m => m.Useremail == user.Useremail && m.UserPassword == user.UserPassword)
                .FirstOrDefault();

            if (check_user_authentication != null)
            {
                if (check_user_authentication.Isactive == "InActive")
                {
                    return Ok(new { message = "InActive" });
                }
                else
                {
                    var gettoken = GenerateJwtToken(check_user_authentication);
                    var get_store_by_id = db.Stores
                        .Where(m => m.StoreId == check_user_authentication.StoreId)
                        .FirstOrDefault();

                    LoginHistory  loginHistory= new LoginHistory();

                    loginHistory.LogName = check_user_authentication.UserName;
                    loginHistory.StoreId = check_user_authentication.StoreId.ToString();
                    loginHistory.TransactionDt = PST();
                    db.LoginHistories.Add(loginHistory);
                    db.SaveChanges();
                    return Ok(new { token = gettoken, storedata = get_store_by_id, userdata = check_user_authentication });
                }
            }
            else
            {
                return BadRequest(new { message = "Invalid Credentials" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            DateTime utcNow = DateTime.UtcNow;

            // Retrieve static secret key from configuration
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("userId", user.UserId.ToString())
    };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: utcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])), // Use UTC for JWT
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



    }
}




