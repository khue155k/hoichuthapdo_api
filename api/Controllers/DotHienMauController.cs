using API.Common;
using API.Controllers;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DotHienMauController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly OneSignalService _oneSignalService;
        private readonly HttpClient _httpClient;

        public DotHienMauController(ApplicationDbContext context, OneSignalService oneSignalService, HttpClient httpClient)
        {
            _context = context;
            _oneSignalService = oneSignalService;
            _httpClient = httpClient;
        }
        // POST: https://localhost:7037/api/Register
        /// <summary>
        /// Tạo đợt hiến máu
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost("createDotHm")]
        public async Task<ActionResult<TemplateResult<DotHienMau>>> CreateDotHm([FromBody] DotHienMau dotHienMau)
        {
            var result = new TemplateResult<DotHienMau> {};
            if (!ModelState.IsValid)
            {
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }
            _context.dot_hien_mau.Add(dotHienMau);
            _context.SaveChanges();

            var threeMonthsAgo = dotHienMau.ThoiGianBatDau.AddMonths(-3);

            var dsTNV = _context.tinh_nguyen_vien
                .Where(tnv => !string.IsNullOrEmpty(tnv.OneSiginal_ID))
                .Where(tnv => _context.tt_hien_mau
                    .Where(tt => tt.CCCD == tnv.CCCD)
                    .OrderByDescending(tt => tt.ThoiGianHien)
                    .Select(tt => tt.ThoiGianHien)
                    .FirstOrDefault() <= threeMonthsAgo)
            .ToList();

            var thongBao = new ThongBao
            {
                TieuDe = "Đợt hiến máu mới",
                NoiDung = $"Bạn đủ điều kiện tham gia {dotHienMau.TenDot} tại {dotHienMau.DiaDiem} ({dotHienMau.ThoiGianBatDau} - {dotHienMau.ThoiGianKetThuc}). Hãy đăng ký ngay nào!",
            };
            var CreateThongBaoRequest = new CreateThongBaoRequestDto
            {
                ThongBao = thongBao,
                DSTinhNguyenVien = dsTNV
            };

            var controller = new ThongBaoController(_context,_oneSignalService); 
            await controller.CreateThongBaoToList(CreateThongBaoRequest);

            result.Code = 200;
            result.Message = "Tạo đợt hiến máu thành công";
            result.Data = dotHienMau;
            return result;
        }

        [Authorize(Roles = "admin")]
        [HttpPut("updateDotHm/{id}")]
        public async Task<ActionResult<TemplateResult<DotHienMau>>> UpdateDotHm(ulong id, [FromBody] DotHienMau dotHienMau)
        {
            var result = new TemplateResult<DotHienMau> { };

            var existingEntry = _context.dot_hien_mau.FirstOrDefault(d => d.MaDot == id);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy đợt hiến máu có id = {id}";
                return result;
            }
            existingEntry.TenDot = dotHienMau.TenDot;
            existingEntry.DiaDiem = dotHienMau.DiaDiem;
            existingEntry.ThoiGianBatDau = dotHienMau.ThoiGianBatDau;
            existingEntry.ThoiGianKetThuc = dotHienMau.ThoiGianKetThuc;
            existingEntry.DonViMau = dotHienMau.DonViMau;
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa đợt hiến máu thành công";
            result.Data = existingEntry;
            return result;
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("deleteDotHm/{id}")]
        public async Task<ActionResult<TemplateResult<object>>> DeleteDotHm(ulong id)
        {
            var result = new TemplateResult<object> { };
            var existingEntry = _context.dot_hien_mau.FirstOrDefault(d => d.MaDot == id);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy đợt hiến máu có id = {id}";
                return result;
            }

            _context.dot_hien_mau.Remove(existingEntry);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Xóa đợt hiến máu thành công";
            return result;
        }

        // GET: api/dothienmau/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateResult<DotHienMau>>> GetDotHienMau(ulong id)
        {
            var dotHienMau = await _context.dot_hien_mau.FindAsync(id);
            var result = new TemplateResult<DotHienMau>();

            if (dotHienMau == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy đợt hiến máu thành công";
            result.Data = dotHienMau;
            return result;
        }

        // GET: api/DotHienMau/search
        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<DotHienMau>>>> Search(
            string string_tim_kiem = "Nội dung tìm kiếm",
            int pageSize = 10,
            int currentPage = 1)
        {
            var query = _context.dot_hien_mau
                .OrderByDescending(d => d.ThoiGianBatDau)
                .AsQueryable();

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.TenDot.Contains(string_tim_kiem) ||
                                         q.DiaDiem.Contains(string_tim_kiem));          
            }

            var result = new TemplateResult<PaginatedResult<DotHienMau>>();

            var totalCount = query.Count();

            var data = await query
             .Skip((currentPage - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync();

            var paginatedResult = new PaginatedResult<DotHienMau>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data,
            };

            result.Code = 200;
            result.Message = "Tìm kiếm danh sách đợt hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        // GET: api/DotHieMmau/TheTichMH
        [HttpGet("TheTichMH")]
        public async Task<ActionResult<TemplateResult<IEnumerable<object>>>> GetAllTheTichMauHien()
        {
            var theTichMauHienList = await (from theTich in _context.the_tich_mau_hien
                                            select new
                                            {
                                                value = theTich.MaTheTich,
                                                label = theTich.TheTich
                                            })
                                            .ToListAsync();
            return Ok(new TemplateResult<IEnumerable<object>>
            {
                Code = 200,
                Message = "Lấy danh sách thể tích máu hiến thành công",
                Data = theTichMauHienList
            });
        }

        // GET: api/DotHieMmau
        [HttpGet]
        public async Task<ActionResult<TemplateResult<IEnumerable<DotHienMau>>>> GetAllDotHienMau()
        {
            var dotHienMauList = await _context.dot_hien_mau.OrderByDescending(d => d.ThoiGianBatDau).ToListAsync();

            var result = new TemplateResult<IEnumerable<DotHienMau>>();

            if (dotHienMauList == null || dotHienMauList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy danh sách đợt hiến máu thành công";
            result.Data = dotHienMauList;

            return result;
        }

        [HttpGet("getDotHMsPaginated")]
        public async Task<ActionResult<TemplateResult<IEnumerable<DotHienMau>>>> GetAllDotHienMauPaginated(int pageSize = 10, int currentPage = 1)
        {
            var dotHienMauList = await _context.dot_hien_mau
                .OrderByDescending(d => d.ThoiGianBatDau)
                .ToListAsync();

            var result = new TemplateResult<PaginatedResult<DotHienMau>>();

            if (dotHienMauList == null || dotHienMauList.Count == 0)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu";
                return Ok(result);
            }

            var totalCount = dotHienMauList.Count();
            var data = dotHienMauList
             .Skip((currentPage - 1) * pageSize)
             .Take(pageSize);

            var paginatedResult = new PaginatedResult<DotHienMau>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Lấy danh sách đợt hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        [HttpGet("getNamHMs")]
        public async Task<ActionResult<TemplateResult<object>>> GetNamHMs()
        {
            var NamHMs = await _context.dot_hien_mau
                .GroupBy(d => new { d.ThoiGianBatDau.Year })
                .OrderByDescending(g => g.Key.Year)
                .Select(g => new
                {
                    nam = g.Key.Year
                })
                .ToListAsync();

            return Ok(new TemplateResult<object>
            {
                Code = 200,
                Message = "Lấy danh sách năm hiến máu thành công",
                Data = NamHMs
            });
        }
    }
}
