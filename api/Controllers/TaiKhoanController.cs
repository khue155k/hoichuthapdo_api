using api.Common;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Telegram.BotAPI.AvailableTypes;

namespace api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public TaiKhoanController(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<TemplateResult<IEnumerable<TaiKhoan>>>> GetTaiKhoan(int pageSize = 10, int currentPage = 1)
        {
            var result = new TemplateResult<IEnumerable<TaiKhoan>>();

            var userList = await _context.tai_khoan
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (userList == null || userList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy tài khoản nào";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy danh sách tài khoản";
            result.Data = userList;

            return result;
        }

        [Authorize(Roles = "admin")]
        [HttpGet("TTQTV")]
        public async Task<ActionResult<TemplateResult<QuanTriVien>>> GetTTQTV(string id)
        {
            var result = new TemplateResult<QuanTriVien>();

            var QTV = await _context.quan_tri_vien.FirstOrDefaultAsync(qtv => qtv.TaiKhoan_ID == id);

            if (QTV == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy quản trị viên nào";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy thông tin quản trị viên thành công";
            result.Data = QTV;

            return result;
        }
        [Authorize(Roles = "admin")]
        [HttpPut("updateQTV/{id}")]
        public async Task<ActionResult<TemplateResult<QuanTriVien>>> UpdateQTV(ulong id, [FromBody] QuanTriVien quanTriVien)
        {
            var result = new TemplateResult<QuanTriVien> { };

            var existingEntry = _context.quan_tri_vien.FirstOrDefault(d => d.MaQTV == id);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy quản trị viên có id = {id}";
                return result;
            }
            existingEntry.TenQTV = quanTriVien.TenQTV;
            existingEntry.BoPhan = quanTriVien.BoPhan;
            existingEntry.ChucVu = quanTriVien.ChucVu;
            existingEntry.Email = quanTriVien.Email;
            existingEntry.SoDienThoai = quanTriVien.SoDienThoai;
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa quản trị viênthành công";
            result.Data = existingEntry;
            return result;
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<TaiKhoan>>>> SearchTaiKhoan(string string_tim_kiem = "Nội dung tìm kiếm", int pageSize = 10, int currentPage = 1)
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<TaiKhoan>>();
            var usersInUserRole = await userManager.GetUsersInRoleAsync("user");

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                usersInUserRole = usersInUserRole.Where(q => q.UserName.Contains(string_tim_kiem)).ToList();
            }

            var totalCount =  usersInUserRole.Count();
            var users =  usersInUserRole
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginatedResult = new PaginatedResult<TaiKhoan>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = users
            };

            var result = new TemplateResult<PaginatedResult<TaiKhoan>>();

            result.Code = 200;
            result.Message = "Lấy danh sách tài khoản";
            result.Data = paginatedResult;

            return result;
        }
        [Authorize]
        [HttpPut("resetPassword/{id}")]
        public async Task<ActionResult<TemplateResult<string>>> ResetPassword(string id)
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<TaiKhoan>>();
            var user = await userManager.FindByIdAsync(id);

            var result = new TemplateResult<string>();

            if (user == null)
            {
                result.Code = 404;
                result.Message  = $"Không tìm thấy người dùng có {id}";
                return result;
            }
            string resetPassword = "User1@";
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var result1 = await userManager.ResetPasswordAsync(user, token, resetPassword);

            if (!result1.Succeeded)
            {
                result.Code = 400;
                result.Message = string.Join("; ", result1.Errors.Select(e => e.Description));
                return result;
            }

            result.Code = 200;
            result.Message = "Đổi mật khẩu thành công";
            result.Data = resetPassword;

            return result;
        }

    }
}
