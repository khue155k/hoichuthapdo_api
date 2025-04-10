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
        [Authorize]
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
            result.Message = "Tạo đơn vị hiến máu thành công";
            result.Data = quaTang;
            return result;
        }
        [Authorize]
        [HttpPut("updateQuaTang/{id}")]
        public async Task<ActionResult<TemplateResult<QuaTang>>> UpdateQuaTang(ulong id, [FromBody] QuaTang quaTang)
        {
            var result = new TemplateResult<QuaTang> { };

            var existingEntry = _context.qua_tang.FirstOrDefault(d => d.MaQua == id);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy đơn vị hiến máu có id = {id}";
                return result;
            }
            existingEntry.TenQua = quaTang.TenQua;
            existingEntry.GiaTri = quaTang.GiaTri;

            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa đơn vị hiến máu thành công";
            result.Data = existingEntry;
            return result;
        }
        [Authorize]
        [HttpDelete("deleteQuaTang/{id}")]
        public async Task<ActionResult<TemplateResult<object>>> DeleteQuaTang(ulong id)
        {
            var result = new TemplateResult<object> { };
            var existingEntry = _context.qua_tang.FirstOrDefault(d => d.MaQua == id);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy đơn vị hiến máu có id = {id}";
                return result;
            }

            _context.qua_tang.Remove(existingEntry);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Xóa đơn vị hiến máu thành công";
            return result;
        }

        // GET: api/donvi/{id}
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
            result.Message = "Lấy đơn vị hiến máu thành công";
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
            result.Message = "Tìm kiếm danh sách đơn vị hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        // GET: api/QuaTang
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
            result.Message = "Lấy danh sách đơn vị hiến máu thành công";
            result.Data = quaTangList;

            return result;
        }

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
            result.Message = "Lấy danh sách đơn vị hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }
    }
}
