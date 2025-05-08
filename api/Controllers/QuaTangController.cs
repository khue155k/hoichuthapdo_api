using API.Common;
using API.Controllers;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuaTangController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuaTangController(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Tạo quà tặng
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost("createQuaTang")]
        public async Task<ActionResult<TemplateResult<QuaTang>>> CreateQuaTang([FromBody] QuaTang quaTang)
        {
            var result = new TemplateResult<QuaTang> { };
            if (!ModelState.IsValid)
            {
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }
            _context.qua_tang.Add(quaTang);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tạo quà tặng thành công";
            result.Data = quaTang;
            return result;
        }
        [Authorize(Roles = "admin")]
        [HttpPut("updateQuaTang/{id}")]
        public async Task<ActionResult<TemplateResult<QuaTang>>> UpdateQuaTang(ulong id, [FromBody] QuaTang quaTang)
        {
            var result = new TemplateResult<QuaTang> { };

            var existingEntry = _context.qua_tang.FirstOrDefault(d => d.MaQua == id);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy quà tặng có id = {id}";
                return result;
            }
            existingEntry.TenQua = quaTang.TenQua;
            existingEntry.GiaTri = quaTang.GiaTri;

            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa quà tặng thành công";
            result.Data = existingEntry;
            return result;
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("deleteQuaTang/{id}")]
        public async Task<ActionResult<TemplateResult<object>>> DeleteQuaTang(ulong id)
        {
            var result = new TemplateResult<object> { };
            var existingEntry = _context.qua_tang.FirstOrDefault(d => d.MaQua == id);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy quà tặng có id = {id}";
                return result;
            }

            _context.qua_tang.Remove(existingEntry);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Xóa quà tặng thành công";
            return result;
        }

        // GET: api/QuaTang/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateResult<QuaTang>>> GetQuaTang(ulong id)
        {
            var quaTang = await _context.qua_tang.FindAsync(id);
            var result = new TemplateResult<QuaTang>();

            if (quaTang == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy quà tặng thành công";
            result.Data = quaTang;
            return result;
        }

        // GET: api/QuaTang/search
        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<QuaTang>>>> Search(
            string string_tim_kiem = "Nội dung tìm kiếm",
            int pageSize = 10,
            int currentPage = 1)
        {
            var query = _context.qua_tang.AsEnumerable();

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.TenQua.Contains(string_tim_kiem));
            }

            var result = new TemplateResult<PaginatedResult<QuaTang>>();

            var totalCount = query.Count();
            var data = query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<QuaTang>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Tìm kiếm danh sách quà tặng thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        // GET: api/QuaTang
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<TemplateResult<IEnumerable<QuaTang>>>> GetAllQuaTang()
        {
            var quaTangList = await _context.qua_tang.ToListAsync();

            var result = new TemplateResult<IEnumerable<QuaTang>>();

            if (quaTangList == null || quaTangList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy danh sách quà tặng thành công";
            result.Data = quaTangList;

            return result;
        }
        [Authorize]
        [HttpGet("getQuaTangsPaginated")]
        public async Task<ActionResult<TemplateResult<IEnumerable<QuaTang>>>> GetAllQuaTangPaginated(int pageSize = 10, int currentPage = 1)
        {
            var quaTangList = await _context.qua_tang.ToListAsync();

            var result = new TemplateResult<PaginatedResult<QuaTang>>();

            if (quaTangList == null || quaTangList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return Ok(result);
            }

            var totalCount = quaTangList.Count();
            var data = quaTangList
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<QuaTang>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Lấy danh sách quà tặng thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        [Authorize]
        [HttpGet("searchTNV")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<TinhNguyenVienDTO>>>> SearchTNV(
            string string_tim_kiem = "Nội dung tìm kiếm",
            int pageSize = 10,
            int currentPage = 1)
        {
            var query = _context.tinh_nguyen_vien.AsQueryable();

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.HoTen.Contains(string_tim_kiem) ||
                                         q.SoDienThoai.Contains(string_tim_kiem) ||
                                         q.CCCD.Contains(string_tim_kiem) ||
                                         q.Email.Contains(string_tim_kiem));
            }

            var totalCount = await query.CountAsync();

            var data = await query.ToListAsync();

            var cccds = data.Select(x => x.CCCD).ToList();

            var quaDaNhanCounts = await _context.lich_su_tang_qua
                .Where(x => cccds.Contains(x.CCCD))
                .GroupBy(x => x.CCCD)
                .Select(g => new { CCCD = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CCCD, x => x.Count);

            var dtoList = data.Select(tnv => new TinhNguyenVienDTO
            {
                CCCD = tnv.CCCD,
                HoTen = tnv.HoTen,
                SoDienThoai = tnv.SoDienThoai,
                Email = tnv.Email,
                MaTinhThanh = tnv.MaTinhThanh,
                MaQuanHuyen = tnv.MaQuanHuyen,
                MaPhuongXa = tnv.MaPhuongXa,
                SoLanHien = tnv.SoLanHien,
                SoQuaDaNhan = quaDaNhanCounts.ContainsKey(tnv.CCCD) ? quaDaNhanCounts[tnv.CCCD] : 0
            }).ToList();

            var sortedList = dtoList
                .OrderBy(x => x.SoQuaDaNhan)
                .ThenByDescending(x => x.SoLanHien)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginatedResult = new PaginatedResult<TinhNguyenVienDTO>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = sortedList
            };

            var result = new TemplateResult<PaginatedResult<TinhNguyenVienDTO>>
            {
                Code = 200,
                Message = "Tìm kiếm danh sách tình nguyện viên thành công",
                Data = paginatedResult
            };

            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("tangQua")]
        public async Task<ActionResult<TemplateResult<LichSuTangQua>>> TangQua([FromBody] LichSuTangQua lichSuTangQua)
        {
            var result = new TemplateResult<LichSuTangQua> { };
            if (!ModelState.IsValid)
            {
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }
            lichSuTangQua.ThoiGianGui = DateTime.Now;
            _context.lich_su_tang_qua.Add(lichSuTangQua);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tặng quà thành công";
            result.Data = lichSuTangQua;
            return result;
        }

    }
}
