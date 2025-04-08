using API.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using api.Common;
using api.Models;
using Microsoft.Extensions.Options;

namespace API.Controllers
{
    /// <summary>
    /// Controller đăng nhập.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public LoginController(ApplicationDbContext context, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _jwtSettings = jwtOptions.Value;

        }

        // POST: https://localhost:7037/api/Login
        /// <summary>
        /// Đăng nhập vào hệ thống.
        /// </summary>
        /// <param name="loginUser">Thông tin đăng nhập bao gồm tài khoản và mật khẩu.</param>
        /// <returns>Trả về JWT token nếu đăng nhập thành công.</returns>
        [HttpPost]
        public async Task<ActionResult<TemplateResult<object>>> Login([FromBody] Login loginUser)
        {
            string hashedPassword = HashPassword(loginUser.Password);

            var user = (from u in _context.tai_khoan
                        where u.Username == loginUser.Username
                        && u.Password == hashedPassword
                        select u).FirstOrDefault();

            if (user == null)
            {
                return Unauthorized(new TemplateResult<object> { Code = 200, Message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new TemplateResult<object>
            {
                Code = 200,
                Message = "Login complete!",
                Data = new
                {
                    token = tokenString
                }
            });
        }

        // POST: api/Login/ChangePassword
        /// <summary>
        /// Đổi mật khẩu.
        /// </summary>
        /// <param name="request">Thông tin đổi mật khẩu bao gồm Username, mật khẩu cũ và mật khảu mới.</param>
        /// <returns>Trả về JWT token nếu đăng nhập thành công.</returns>
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<ActionResult<TemplateResult<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = _context.tai_khoan.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                return Ok(new TemplateResult<object>
                {
                    Code = 404,
                    Message = "Không tìm thấy người dùng",
                });
            }

            string hashedOldPassword = HashPassword(request.oldPassword);
            if (user.Password != hashedOldPassword)
            {
                return Unauthorized(new TemplateResult<object>
                {
                    Code = 200,
                    Message = "Mật khẩu cũ không chính xác.",
                });
            }

            user.Password = HashPassword(request.newPassword);
            await _context.SaveChangesAsync();

            return Ok(new TemplateResult<object>
            {
                Code = 200,
                Message = "Đổi mật khẩu thành công!",
            });
        }

        private string HashPassword(string Password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
    public class Register
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class Login
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class ChangePasswordRequest
    {
        public string Username { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
    }
}
