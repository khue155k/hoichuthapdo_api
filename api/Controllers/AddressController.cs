using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AddressController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var result = await _context.Provinces.ToListAsync();

            return Ok(result);
        }

        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts()
        {
            var result = await _context.Districts.ToListAsync();

            return Ok(result);
        }

        [HttpGet("wards")]
        public async Task<IActionResult> GetWards()
        {
            var result = await _context.Wards.ToListAsync();

            return Ok(result);
        }

        [HttpGet("districts/provinceId")]
        public async Task<IActionResult> GetDistrictsByProvinceId(uint provinceId)
        {
            var result = await _context.Districts.Where(d => d.ProvinceId == provinceId).ToListAsync();

            return Ok(result);
        }

        [HttpGet("wards/districtId")]
        public async Task<IActionResult> GetWardsByDistrictId(uint districtId)
        {
            var result = await _context.Wards.Where(w => w.DistrictId == districtId).ToListAsync();

            return Ok(result);
        }

        [HttpGet("province")]
        public async Task<IActionResult> GetProvince(uint provinceId)
        {
            var result = await _context.Provinces.FirstOrDefaultAsync(p => p.ProvinceId == provinceId);

            return Ok(result);
        }

        [HttpGet("district")]
        public async Task<IActionResult> GetDistrict(uint districtId)
        {
            var result = await _context.Districts.FirstOrDefaultAsync(p => p.DistrictId == districtId);

            return Ok(result);
        }

        [HttpGet("ward")]
        public async Task<IActionResult> GetWard(uint wardId)
        {
            var result = await _context.Wards.FirstOrDefaultAsync(p => p.WardId == wardId);

            return Ok(result);
        }
    }
}
