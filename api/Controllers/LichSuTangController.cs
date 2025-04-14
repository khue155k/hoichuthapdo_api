using api.Common;
using API.Controllers;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LichSuTangQuaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LichSuTangQuaController(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Tạo lịch sử tặng quà
        /// </summary>
        [Authorize]
        [HttpPost("createLSTQ")]
        public async Task<ActionResult<TemplateResult<LichSuTangQua>>> CreateLSTQ([FromBody] LichSuTangQua lichSu)
        {
            var result = new TemplateResult<LichSuTangQua> { };
            if (!ModelState.IsValid)
            {
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }
            lichSu.ThoiGianGui = DateTime.Now;
            _context.lich_su_tang_qua.Add(lichSu);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tạo lịch sử tặng quà thành công";
            result.Data = lichSu;
            return result;
        }
        [Authorize]
        [HttpPut("updateLSTQ")]
        public async Task<ActionResult<TemplateResult<LichSuTangQua>>> UpdatLSTQ(string cccd, ulong maQua, [FromBody] LichSuTangQua lichSu)
        {
            var result = new TemplateResult<LichSuTangQua> { };

            var existingEntry = _context.lich_su_tang_qua.FirstOrDefault(d => d.MaQua == maQua && d.CCCD == cccd);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy lịch sử tặng quà";
                return result;
            }
            existingEntry.NoiDung = lichSu.NoiDung;

            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa lịch sử tặng quà thành công";
            result.Data = existingEntry;
            return result;
        }
        [Authorize]
        [HttpDelete("deleteLSTQ")]
        public async Task<ActionResult<TemplateResult<object>>> DeleteLSTQ(string cccd, ulong maQua)
        {
            var result = new TemplateResult<object> { };
            var existingEntry = _context.lich_su_tang_qua.FirstOrDefault(d => d.MaQua == maQua && d.CCCD == cccd);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy lịch sử tặng quà";
                return result;
            }

            _context.lich_su_tang_qua.Remove(existingEntry);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Xóa lịch sử tặng quà thành công";
            return result;
        }

        // GET: api/LichSuTangQua/{id}
        [Authorize]
        [HttpGet("getLSTQ")]
        public async Task<ActionResult<TemplateResult<LichSuTangQua>>> GetLSTQ(string cccd, ulong maQua)
        {
            var lichSu = await _context.lich_su_tang_qua.FirstOrDefaultAsync(d => d.MaQua == maQua && d.CCCD == cccd);
            var result = new TemplateResult<LichSuTangQua>();

            if (lichSu == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy lịch sử tặng quà thành công";
            result.Data = lichSu;
            return result;
        }

        // GET: api/LichSuTangQua/search
        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<LichSuTangQua>>>> Search(
            string string_tim_kiem = "Nội dung tìm kiếm",
            int pageSize = 10,
            int currentPage = 1)
        {
            var query = _context.lich_su_tang_qua.AsEnumerable();

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.NoiDung.Contains(string_tim_kiem) ||
                                         q.CCCD.Contains(string_tim_kiem) ||
                                         q.MaQua.ToString().Contains(string_tim_kiem));
            }

            var result = new TemplateResult<PaginatedResult<LichSuTangQua>>();

            var totalCount = query.Count();
            var data = query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<LichSuTangQua>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Tìm kiếm danh sách lịch sử tặng quà thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        // GET: api/LichSuTangQua
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<TemplateResult<IEnumerable<LichSuTangQua>>>> GetAllLichSuTangQua()
        {
            var lichSuList = await _context.lich_su_tang_qua.ToListAsync();

            var result = new TemplateResult<IEnumerable<LichSuTangQua>>();

            if (lichSuList == null || lichSuList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy danh sách lịch sử tặng quà thành công";
            result.Data = lichSuList;

            return result;
        }

        [HttpGet("getLSTQsPaginated")]
        public async Task<ActionResult<TemplateResult<IEnumerable<LichSuTangQua>>>> GetAllLichSuTangQuaPaginated(int pageSize = 10, int currentPage = 1)
        {
            var lichSuList = await _context.lich_su_tang_qua.ToListAsync();

            var result = new TemplateResult<PaginatedResult<LichSuTangQua>>();

            if (lichSuList == null || lichSuList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return Ok(result);
            }

            var totalCount = lichSuList.Count();
            var data = lichSuList
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<LichSuTangQua>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Lấy danh sách lịch sử tặng quà thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }
    }
}
