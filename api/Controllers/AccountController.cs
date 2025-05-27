using API.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using API.Common;
using API.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using API.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.BotAPI.AvailableTypes;
using System;
using API.Service;

namespace API.Controllers
{
    /// <summary>
    /// Controller đăng nhập.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly EmailSender _emailSender;
        private readonly ThongBaoTuDongService _thongBaoTuDongService;

        public AccountController(ApplicationDbContext context, IOptions<JwtSettings> jwtOptions, IServiceProvider serviceProvider, EmailSender emailSender, ThongBaoTuDongService thongBaoTuDongService)
        {
            _context = context;
            _jwtSettings = jwtOptions.Value;
            _serviceProvider = serviceProvider;
            _emailSender = emailSender;
            _thongBaoTuDongService = thongBaoTuDongService;
        }

        // POST: https://localhost:7037/api/Account/Login
        /// <summary>
        /// Đăng nhập vào hệ thống.
        /// </summary>
        /// <param name="loginUser">Thông tin đăng nhập bao gồm tài khoản và mật khẩu.</param>
        /// <returns>Trả về JWT token nếu đăng nhập thành công.</returns>
        [HttpPost("Login")]
        public async Task<ActionResult<TemplateResult<object>>> Login([FromBody] Login loginUser)
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<TaiKhoan>>();
            var signInManager = _serviceProvider.GetRequiredService<SignInManager<TaiKhoan>>();

            var user = await userManager.FindByNameAsync(loginUser.Username);

            if (user == null)
            {
                return Ok(new TemplateResult<object>
                {
                    Code = 400,
                    Message = "Tài khoản hoặc mật khẩu không chính xác!"
                });
            }

            var result = await signInManager.PasswordSignInAsync(user, loginUser.Password, false, false);

            if (!result.Succeeded)
            {
                return Ok(new TemplateResult<object>
                {
                    Code = 400,
                    Message = "Tài khoản hoặc mật khẩu không chính xác!"
                });
            }

            if (!user.EmailConfirmed)
            {
                _context.tai_khoan.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(new TemplateResult<object>
                {
                    Code = 400,
                    Message = "Bạn cần xác nhận email trước khi đăng nhập. Vui lòng đăng ký lại",
                });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var roles = await userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }.Concat(roleClaims)),

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

        // POST: api/Account/ChangePassword
        /// <summary>
        /// Đổi mật khẩu.
        /// </summary>
        /// <param name="request">Thông tin đổi mật khẩu bao gồm Username, mật khẩu cũ và mật khảu mới.</param>
        /// <returns>Trả về JWT token nếu đăng nhập thành công.</returns>
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<ActionResult<TemplateResult<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = _context.tai_khoan.FirstOrDefault(u => u.UserName == request.Username);
            if (user == null)
            {
                return Ok(new TemplateResult<object>
                {
                    Code = 404,
                    Message = "Không tìm thấy người dùng",
                });
            }
            var userManager = _serviceProvider.GetRequiredService<UserManager<TaiKhoan>>();

            var result = await userManager.ChangePasswordAsync(user, request.oldPassword, request.newPassword);

            if (!result.Succeeded)
            {
                return Ok(new TemplateResult<object>
                {
                    Code = 400,
                    Message = string.Join("; ", result.Errors.Select(e => e.Description)),
                });
            }


            await _context.SaveChangesAsync();

            return Ok(new TemplateResult<object>
            {
                Code = 200,
                Message = "Đổi mật khẩu thành công!",
            });
        }

        [HttpPost("Register")]
        public async Task<ActionResult<TemplateResult<object>>> Register([FromBody] RegisterDto request)
        {
            if (await _context.tai_khoan.AnyAsync(x => x.UserName == request.Username))
            {
                return Ok(new TemplateResult<object>
                {
                    Code = 400,
                    Message = "Tài khoản đã tồn tại",
                });
            }

            var userManager = _serviceProvider.GetRequiredService<UserManager<TaiKhoan>>();

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                var newUser = new TaiKhoan
                {
                    CCCD = request.CCCD,
                    UserName = request.Username,
                    Email = request.Email,
                    EmailConfirmed = false,
                    EmailVerificationCode = Guid.NewGuid().ToString().Substring(0, 6),
                    Create_time = DateTime.Now,
                };

                var result = await userManager.CreateAsync(newUser, request.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "user");

                    await _emailSender.SendEmailAsync(
                        newUser.Email,
                        "Mã xác nhận",
                        $"Mã xác nhận của bạn là: {newUser.EmailVerificationCode}"
                    );

                    return Ok(new TemplateResult<object>
                    {
                        Code = 200,
                        Message = "Vui lòng kiểm tra email để xác nhận tài khoản",
                    });
                }
                else
                {
                    return Ok(new TemplateResult<object>
                    {
                        Code = 400,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description)),
                    });
                }
            }

            return Ok(new TemplateResult<object>
            {
                Code = 400,
                Message = "Email đã tồn tại",
            });
        }

        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == model.Username &&
                x.EmailVerificationCode == model.VerificationCode);

            if (user == null)
            {
                return Ok(new TemplateResult<object>
                {
                    Code = 400,
                    Message = "Mã xác nhận không đúng hoặc tài khoản không tồn tại",
                });
            }

            user.EmailConfirmed = true;
            user.EmailVerificationCode = null;

            var tnv = await _context.tinh_nguyen_vien.FirstOrDefaultAsync(x => x.CCCD == user.CCCD);
            if (tnv != null)
            {
				tnv.TaiKhoan_ID = user.Id;
			}

			await _context.SaveChangesAsync();

            return Ok(new TemplateResult<object>
            {
                Code = 200,
                Message = "Xác nhận email thành công, bạn có thể đăng nhập.",
            });
        }
        [Authorize]
        [HttpGet("AccId/{AccId}")]
        public async Task<ActionResult<TemplateResult<object>>> GetCCCDByAccId(string AccId)
        {
            var result = new TemplateResult<object> { };

            var taiKhoan = await _context.tai_khoan
                                               .FirstOrDefaultAsync(tnv => tnv.Id == AccId);

            if (taiKhoan == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy CCCD với mã tài khoản đã cho.";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy thông tin thành công.";
            result.Data = taiKhoan.CCCD;
            return result;
        }

        [Authorize(Roles = "admin")]
        [HttpPost("CreateAdmin")]
        public async Task<ActionResult<TemplateResult<object>>> CreateAdmin ([FromBody] CreateAdminDto request)
        {
            if (await _context.tai_khoan.AnyAsync(x => x.UserName == request.Username))
            {
                return Ok(new TemplateResult<object>
                {
                    Code = 400,
                    Message = "Tên tài khoản đã tồn tại",
                });
            }

            var userManager = _serviceProvider.GetRequiredService<UserManager<TaiKhoan>>();

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                var newUser = new TaiKhoan
                {
                    UserName = request.Username,
                    Email = request.Email,
                    EmailConfirmed = true,
                    Create_time = DateTime.Now,
                };


                var result = await userManager.CreateAsync(newUser, request.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "admin");

                    _context.quan_tri_vien.Add(new QuanTriVien
                    {
                        TenQTV = request.TenQTV,
                        ChucVu = request.ChucVu,
                        BoPhan = request.BoPhan,
                        Email = request.Email,
                        SoDienThoai = request.SoDienThoai,
                        TaiKhoan_ID = newUser.Id,
                    });
                    await _context.SaveChangesAsync();

                    return Ok(new TemplateResult<object>
                    {
                        Code = 200,
                        Message = "Tạo tài khoản admin thành công",
                    });
                }
                else
                {
                    return Ok(new TemplateResult<object>
                    {
                        Code = 400,
                        Message = string.Join("; ", result.Errors.Select(e => e.Description)),
                    });
                }
            }

            return Ok(new TemplateResult<object>
            {
                Code = 400,
                Message = "Email đã tồn tại",
            });
        }
    }

    public class VerifyEmailDto
    {
        public string Username { get; set; }
        public string VerificationCode { get; set; }
    }

    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string CCCD { get; set; }
        public string Password { get; set; }
    }
    public class CreateAdminDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TenQTV { get; set; }
        public string ChucVu { get; set; }
        public string BoPhan { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
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
