using api.Common;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinhNguyenVienController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TinhNguyenVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/TinhNguyenVien
        [HttpPost]
        public async Task<ActionResult<TemplateResult<object>>> CreateTinhNguyenVien([FromBody] TinhNguyenVien tinhNguyenVien)
        {
            var result = new TemplateResult<object> { };
            if (!ModelState.IsValid)
            {
                //var errors = ModelState.Values.SelectMany(v => v.Errors)
                //                   .Select(e => e.ErrorMessage);
                //return BadRequest(new TemplateResult<object> { Code = 400, Message = string.Join(", ", errors) });
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }

            var existingVolunteer = _context.tinh_nguyen_vien
                                            .FirstOrDefault(tnv => tnv.CCCD == tinhNguyenVien.CCCD);
            if (existingVolunteer != null)
            {
                result.Code = 200;
                result.Message = "Tình nguyện viên đã được tạo từ trước";
                result.Data = new { id = existingVolunteer.CCCD };
                return result;
            }

            var currentDate = DateTime.Now;
            var age = currentDate.Year - tinhNguyenVien.NgaySinh.Year;

            if (currentDate < tinhNguyenVien.NgaySinh.AddYears(age))
            {
                age--;
            }

            if (age < 18)
            {
                result.Code = 400;
                result.Message = "Người hiến máu chưa đủ 18 tuổi.";
                return result;
            }
            _context.tinh_nguyen_vien.Add(tinhNguyenVien);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tạo tình nguyện viên thành công";
            result.Data = new { id = tinhNguyenVien.CCCD };
            return result;
        }

        // GET: api/TinhNguyenVien/cccd/{cccd}
        [HttpGet("cccd/{cccd}")]
        public async Task<ActionResult<TemplateResult<TinhNguyenVien>>> GetTinhNguyenVienByCCCD(string CCCD)
        {
            var result = new TemplateResult<TinhNguyenVien> { };

            var tinhNguyenVien = await _context.tinh_nguyen_vien
                                               .FirstOrDefaultAsync(tnv => tnv.CCCD == CCCD);

            if (tinhNguyenVien == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy tình nguyện viên với CCCD này.";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy thông tin thành công.";
            result.Data = tinhNguyenVien;
            return result;
        }

        [Authorize]
        [HttpPost("createTNV")]
        public async Task<ActionResult<TemplateResult<TinhNguyenVien>>> CreateTNV([FromBody] TinhNguyenVien TNV)
        {
            var result = new TemplateResult<TinhNguyenVien> { };
            if (!ModelState.IsValid)
            {
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }
            _context.tinh_nguyen_vien.Add(TNV);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tạo tình nguyện viên hiến máu thành công";
            result.Data = TNV;
            return result;
        }
        [Authorize]
        [HttpPut("updateTNV/{id}")]
        public async Task<ActionResult<TemplateResult<TinhNguyenVien>>> UpdateTNV(string CCCD, [FromBody] TinhNguyenVien TNV)
        {
            var result = new TemplateResult<TinhNguyenVien> { };

            var existingEntry = _context.tinh_nguyen_vien.FirstOrDefault(d => d.CCCD == CCCD);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy tình nguyện viên hiến máu có id = {CCCD}";
                return result;
            }
            existingEntry.HoTen = TNV.HoTen;
            existingEntry.NgaySinh = TNV.NgaySinh;
            existingEntry.GioiTinh = TNV.GioiTinh;
            existingEntry.SoDienThoai = TNV.SoDienThoai;
            existingEntry.Email = TNV.Email;
            existingEntry.MaTinhThanh = TNV.MaTinhThanh;
            existingEntry.MaQuanHuyen = TNV.MaQuanHuyen;
            existingEntry.MaPhuongXa = TNV.MaPhuongXa;
            existingEntry.SoLanHien = TNV.SoLanHien;
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa tình nguyện viên hiến máu thành công";
            result.Data = existingEntry;
            return result;
        }
        [Authorize]
        [HttpDelete("deleteTNV/{id}")]
        public async Task<ActionResult<TemplateResult<object>>> DeleteTNV(string CCCD)
        {
            var result = new TemplateResult<object> { };
            var existingEntry = _context.tinh_nguyen_vien.FirstOrDefault(d => d.CCCD == CCCD);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy tình nguyện viên hiến máu có id = {CCCD}";
                return result;
            }

            _context.tinh_nguyen_vien.Remove(existingEntry);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Xóa tình nguyện viên hiến máu thành công";
            return result;
        }

        // GET: api/TinhNguyenVien/search
        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<TinhNguyenVien>>>> Search(
            string string_tim_kiem = "Nội dung tìm kiếm",
            int pageSize = 10,
            int currentPage = 1)
        {
            var query = _context.tinh_nguyen_vien.AsEnumerable();

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.HoTen.Contains(string_tim_kiem) ||
                                         q.SoDienThoai.Contains(string_tim_kiem) ||
                                         q.CCCD.Contains(string_tim_kiem) ||
                                         q.Email.Contains(string_tim_kiem));
            }

            var result = new TemplateResult<PaginatedResult<TinhNguyenVien>>();

            var totalCount = query.Count();
            var data = query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<TinhNguyenVien>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Tìm kiếm danh sách tình nguyện viên hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        [HttpGet("getTNVsPaginated")]
        public async Task<ActionResult<TemplateResult<IEnumerable<TinhNguyenVien>>>> GetAllTNVPaginated(int pageSize = 10, int currentPage = 1)
        {
            var TNVList = await _context.tinh_nguyen_vien.ToListAsync();

            var result = new TemplateResult<PaginatedResult<TinhNguyenVien>>();

            if (TNVList == null || TNVList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return Ok(result);
            }

            var totalCount = TNVList.Count();
            var data = TNVList
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<TinhNguyenVien>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Lấy danh sách tình nguyện viên hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }
    }
}
