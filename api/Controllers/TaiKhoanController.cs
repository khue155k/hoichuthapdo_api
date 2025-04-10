using api.Common;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        [Authorize]
        [HttpGet("TTQTV")]
        public async Task<ActionResult<TemplateResult<QuanTriVien>>> GetTTQTV(ulong id)
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

        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<TaiKhoan>>>> SearchtTaiKhoan(string string_tim_kiem = "Nội dung tìm kiếm", int pageSize = 10, int currentPage = 1)
        {
            var query = _context.tai_khoan.AsQueryable();

            query = query.Where(q => q.Role == "user");

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.Username.Contains(string_tim_kiem));
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
        [HttpPut("resetPassword/{id}")]
        public async Task<ActionResult<TemplateResult<string>>> ResetPassword(ulong id)
        {
            var user = await _context.tai_khoan.FirstOrDefaultAsync(u => u.ID == id);

            var result = new TemplateResult<string>();

            if (user == null)
            {
                result.Code = 404;
                result.Message  = $"Không tìm thấy người dùng có {id}";
                return result;
            }
            string resetPassword = "12345678";
            string hashedPassword = HashPassword(resetPassword);

            user.Password = hashedPassword;
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Đổi mật khẩu thành công";
            result.Data = resetPassword;

            return result;
        }


        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
