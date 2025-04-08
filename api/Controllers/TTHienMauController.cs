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
    public class TTHienMauController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TTHienMauController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        [Authorize]
        [HttpGet("ttHMTheoDot")]
        public async Task<ActionResult<TemplateResult<object>>> ttHMTheoDot(int year)
        {
            var data = await _context.dot_hien_mau
                .Where(d => d.ThoiGianBatDau.Year == year)
                .OrderBy(d => d.ThoiGianBatDau)
                .Select(d => new
                {
                    dotHm = d.MaDot,
                    tenDot = d.TenDot,
                    thoiGianBatDau = d.ThoiGianBatDau,
                    thoiGianKetThuc = d.ThoiGianKetThuc,
                    soNguoiDangKy = _context.tt_hien_mau.Count(t => t.MaDot == d.MaDot),
                    soNguoiHienMau = _context.tt_hien_mau.Where(t => t.KetQua == "Đã hiến").Count(t => t.MaDot == d.MaDot)
                }).ToListAsync();

            var result = new TemplateResult<object> { };

            result.Code = 200;
            result.Message = "Lấy danh sách hiến máu theo đợt thành công";
            result.Data = data;

            return result;
        }

        [Authorize]
        [HttpGet("ttHMTheoThang")]
        public async Task<ActionResult<TemplateResult<object>>> ttHMTheoThang(int year)
        {
            var data = await _context.dot_hien_mau
                .Where(d => d.ThoiGianBatDau.Year == year)
                .GroupBy(d => new { d.ThoiGianBatDau.Year, d.ThoiGianBatDau.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = g.Key.Month,
                    SoNguoiDangKy = g.Sum(d => _context.tt_hien_mau.Count(t => t.MaDot == d.MaDot)),
                    SoNguoiHienMau = g.Sum(d => _context.tt_hien_mau.Where(t => t.KetQua == "Đã hiến").Count(t => t.MaDot == d.MaDot))
                })
                .ToListAsync();

            var result = new TemplateResult<object> { };

            result.Code = 200;
            result.Message = "Lấy danh sách hiến máu theo tháng thành công";
            result.Data = data;

            return result;
        }
    }
}
