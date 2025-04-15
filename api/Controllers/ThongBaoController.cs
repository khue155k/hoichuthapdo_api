using api.Common;
using API.Controllers;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongBaoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly OneSignalService _oneSignalService;

        public ThongBaoController(ApplicationDbContext context, OneSignalService oneSignalService)
        {
            _context = context;
            _oneSignalService = oneSignalService;
        }
        /// <summary>
        /// Tạo thông báo
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost("createThongBao")]
        public async Task<ActionResult<TemplateResult<ThongBao>>> CreateThongBao([FromBody] ThongBao ThongBao)
        {
            var result = new TemplateResult<ThongBao> { };
            if (!ModelState.IsValid)
            {
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }
            ThongBao.ThoiGianGui = DateTime.Now;

            _context.thong_bao.Add(ThongBao);
            _context.SaveChanges();

            var dsTNV = await _context.tinh_nguyen_vien.ToListAsync();

            foreach (var tnv in dsTNV)
            {
                _context.thong_bao_TNV.Add(new ThongBao_TinhNguyenVien
                {
                    MaTB = ThongBao.MaTB,
                    CCCD = tnv.CCCD,
                });
            }

            await _oneSignalService.SendNotificationAll(ThongBao.TieuDe, ThongBao.NoiDung);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tạo thông báo thành công";
            result.Data = ThongBao;
            return result;
        }

        [Authorize(Roles = "admin")]
        [HttpPost("createThongBaoToList")]
        public async Task<ActionResult<TemplateResult<ThongBao>>> CreateThongBaoToList([FromBody] CreateThongBaoRequestDto request)
        {
            var thongBao = request.ThongBao;
            var dsTNV = request.DSTinhNguyenVien;

            var result = new TemplateResult<ThongBao> { };
            if (!ModelState.IsValid)
            {
                result.Code = 400;
                result.Message = ModelState.ToString(); 
                return result;
            }
            thongBao.ThoiGianGui = DateTime.Now;

            if (dsTNV == null || dsTNV.Count == 0)
            {
                result.Code = 200;
                result.Message = "";
                result.Data = thongBao;
                return result;
            }

            _context.thong_bao.Add(thongBao);
            _context.SaveChanges();

            foreach (var tnv in dsTNV)
            {
                _context.thong_bao_TNV.Add(new ThongBao_TinhNguyenVien
                {
                    MaTB = thongBao.MaTB,
                    CCCD = tnv.CCCD,
                });
            }

            var listOneSignalId = dsTNV
                .Select(t => t.OneSiginal_ID)
                .ToList();

            await _oneSignalService.SendNotificationList(thongBao.TieuDe, thongBao.NoiDung, listOneSignalId);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tạo thông báo thành công";
            result.Data = thongBao;
            return result;
        }

        [Authorize(Roles = "admin")]
        [HttpPut("updateThongBao/{id}")]
        public async Task<ActionResult<TemplateResult<ThongBao>>> UpdateThongBao(ulong id, [FromBody] ThongBao ThongBao)
        {
            var result = new TemplateResult<ThongBao> { };

            var existingEntry = _context.thong_bao.FirstOrDefault(d => d.MaTB == id);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy thông báo có id = {id}";
                return result;
            }
            existingEntry.TieuDe = ThongBao.TieuDe;
            existingEntry.NoiDung = ThongBao.NoiDung;
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa thông báo thành công";
            result.Data = existingEntry;
            return result;
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("deleteThongBao/{id}")]
        public async Task<ActionResult<TemplateResult<object>>> DeleteThongBao(ulong id)
        {
            var result = new TemplateResult<object> { };
            var existingEntry = _context.thong_bao.FirstOrDefault(d => d.MaTB == id);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy thông báo có id = {id}";
                return result;
            }

            _context.thong_bao.Remove(existingEntry);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Xóa thông báo thành công";
            return result;
        }

        // GET: api/ThongBao/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateResult<ThongBao>>> GetThongBao(ulong id)
        {
            var ThongBao = await _context.thong_bao.FindAsync(id);
            var result = new TemplateResult<ThongBao>();

            if (ThongBao == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy thông báo thành công";
            result.Data = ThongBao;
            return result;
        }

        // GET: api/ThongBao/search
        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<ThongBao>>>> Search(
            string string_tim_kiem = "Nội dung tìm kiếm",
            int pageSize = 10,
            int currentPage = 1)
        {
            var query = _context.thong_bao.OrderByDescending(tb => tb.ThoiGianGui).AsEnumerable();

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.TieuDe.Contains(string_tim_kiem) ||
                                         q.NoiDung.Contains(string_tim_kiem));
            }

            var result = new TemplateResult<PaginatedResult<ThongBao>>();

            var totalCount = query.Count();
            var data = query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<ThongBao>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Tìm kiếm danh sách thông báo thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        // GET: api/ThongBao
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<TemplateResult<IEnumerable<ThongBao>>>> GetAllThongBao()
        {
            var ThongBaoList = await _context.thong_bao.OrderByDescending(tb => tb.ThoiGianGui).ToListAsync();

            var result = new TemplateResult<IEnumerable<ThongBao>>();

            if (ThongBaoList == null || ThongBaoList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy danh sách thông báo thành công";
            result.Data = ThongBaoList;

            return result;
        }
        [Authorize]
        [HttpGet("getThongBaosPaginated")]
        public async Task<ActionResult<TemplateResult<IEnumerable<ThongBao>>>> GetAllThongBaoPaginated(int pageSize = 10, int currentPage = 1)
        {
            var ThongBaoList = await _context.thong_bao.OrderByDescending(tb => tb.ThoiGianGui).ToListAsync();

            var result = new TemplateResult<PaginatedResult<ThongBao>>();

            if (ThongBaoList == null || ThongBaoList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return Ok(result);
            }

            var totalCount = ThongBaoList.Count();
            var data = ThongBaoList
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<ThongBao>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Lấy danh sách thông báo thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }
    }
}
