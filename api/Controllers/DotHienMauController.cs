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
    public class DotHienMauController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DotHienMauController(ApplicationDbContext context)
        {
            _context = context;
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
