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
    public class DonViController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DonViController(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Tạo đơn vị hiến máu
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost("createDonVi")]
        public async Task<ActionResult<TemplateResult<DonVi>>> CreateDonVi([FromBody] DonVi donVi)
        {
            var result = new TemplateResult<DonVi> { };
            if (!ModelState.IsValid)
            {
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }
            _context.don_vi.Add(donVi);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tạo đơn vị hiến máu thành công";
            result.Data = donVi;
            return result;
        }
        [Authorize(Roles = "admin")]
        [HttpPut("updateDonVi/{id}")]
        public async Task<ActionResult<TemplateResult<DonVi>>> UpdateDonVi(ulong id, [FromBody] DonVi donVi)
        {
            var result = new TemplateResult<DonVi> { };

            var existingEntry = _context.don_vi.FirstOrDefault(d => d.MaDV == id);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy đơn vị hiến máu có id = {id}";
                return result;
            }
            existingEntry.TenDV = donVi.TenDV;
            existingEntry.SoDienThoai = donVi.SoDienThoai;
            existingEntry.Email = donVi.Email;
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa đơn vị hiến máu thành công";
            result.Data = existingEntry;
            return result;
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("deleteDonVi/{id}")]
        public async Task<ActionResult<TemplateResult<object>>> DeleteDonVi(ulong id)
        {
            var result = new TemplateResult<object> { };
            var existingEntry = _context.don_vi.FirstOrDefault(d => d.MaDV == id);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy đơn vị hiến máu có id = {id}";
                return result;
            }

            _context.don_vi.Remove(existingEntry);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Xóa đơn vị hiến máu thành công";
            return result;
        }

        // GET: api/donvi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateResult<DonVi>>> GetDonVi(ulong id)
        {
            var donVi = await _context.don_vi.FindAsync(id);
            var result = new TemplateResult<DonVi>();

            if (donVi == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy đơn vị hiến máu thành công";
            result.Data = donVi;
            return result;
        }

        // GET: api/DonVi/search
        [Authorize(Roles = "admin")]
        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<DonVi>>>> Search(
            string string_tim_kiem = "Nội dung tìm kiếm",
            int pageSize = 10,
            int currentPage = 1)
        {
            var query = _context.don_vi.AsEnumerable();

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.TenDV.Contains(string_tim_kiem) ||
                                         q.Email.Contains(string_tim_kiem));
            }

            var result = new TemplateResult<PaginatedResult<DonVi>>();

            var totalCount = query.Count();
            var data = query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<DonVi>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Tìm kiếm danh sách đơn vị hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        // GET: api/DonVi
        [HttpGet]
        public async Task<ActionResult<TemplateResult<IEnumerable<DonVi>>>> GetAllDonVi()
        {
            var donViList = await _context.don_vi.ToListAsync();

            var result = new TemplateResult<IEnumerable<DonVi>>();

            if (donViList == null || donViList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy danh sách đơn vị hiến máu thành công";
            result.Data = donViList;

            return result;
        }

        [HttpGet("getDonVisPaginated")]
        public async Task<ActionResult<TemplateResult<IEnumerable<DonVi>>>> GetAllDonViPaginated(int pageSize = 10, int currentPage = 1)
        {
            var donViList = await _context.don_vi.ToListAsync();

            var result = new TemplateResult<PaginatedResult<DonVi>>();

            if (donViList == null || donViList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return Ok(result);
            }

            var totalCount = donViList.Count();
            var data = donViList
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<DonVi>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Lấy danh sách đơn vị hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }
    }
}
