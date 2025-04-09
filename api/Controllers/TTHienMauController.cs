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

        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateResult<object>>> GetTTHienMauById(ulong id)
        {
            var result = new TemplateResult<object> { };

            var ttHienMau = await (from dsHM in _context.tt_hien_mau
                                   where dsHM.MaTT == id
                                   join dotHM in _context.dot_hien_mau.AsNoTracking() on dsHM.MaDot equals dotHM.MaDot
                                   join tnv in _context.tinh_nguyen_vien.AsNoTracking() on dsHM.CCCD equals tnv.CCCD
                                   join tt in _context.the_tich_mau_hien.AsNoTracking() on dsHM.MaTheTich equals tt.MaTheTich
                                   join dv in _context.don_vi.AsNoTracking() on dsHM.MaDV equals dv.MaDV
                                   join province in _context.Provinces.AsNoTracking() on tnv.MaTinhThanh equals province.ProvinceId
                                   join district in _context.Districts.AsNoTracking() on tnv.MaQuanHuyen equals district.DistrictId
                                   join ward in _context.Wards.AsNoTracking() on tnv.MaPhuongXa equals ward.WardId
                                   select new TTHienMauDto
                                   {
                                       MaTT = dsHM.MaTT,
                                       HoTen = tnv.HoTen,
                                       NgaySinh = tnv.NgaySinh,
                                       GioiTinh = tnv.GioiTinh,
                                       NgheNghiep = dsHM.NgheNghiep,
                                       CCCD = tnv.CCCD,
                                       SoDienThoai = tnv.SoDienThoai,
                                       Email = tnv.Email,
                                       NoiO = dsHM.NoiO,
                                       SoLanHien = tnv.SoLanHien,
                                       KetQua = dsHM.KetQua,
                                       TinhThanh = province.Name,
                                       QuanHuyen = district.Name,
                                       PhuongXa = ward.Name,
                                       MaTinhThanh = tnv.MaTinhThanh,
                                       MaQuanHuyen = tnv.MaQuanHuyen,
                                       MaPhuongXa = tnv.MaPhuongXa,
                                       MaDot = dsHM.MaDot,
                                       TenDot = dotHM.TenDot,
                                       MaDV = dsHM.MaDV,
                                       TenDV = dv.TenDV,
                                       MaTheTich = dsHM.MaTheTich,
                                       TheTich = tt.TheTich,
                                       ThoiGianDangKy = dsHM.ThoiGianDangKy,
                                       ThoiGianHien = dsHM.ThoiGianHien
                                   }).FirstOrDefaultAsync();
            if (ttHienMau == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy nội dung yêu cầu.";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy thông tin hiến máu thành công.";
            result.Data = ttHienMau;
            return result;
        }
        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult<TemplateResult<object>>> UpdateTTHienMau(ulong id, [FromBody] UpdateTTHienMauDto updatedEntry)
        {
            var result = new TemplateResult<object> { };

            var existingEntry = _context.tt_hien_mau
                .FirstOrDefault(d => d.MaTT == id);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy bản ghi để cập nhật.";
                return result;
            }

            var currentDate = DateTime.Now;
            var age = currentDate.Year - updatedEntry.NgaySinh.Year;
            if (currentDate < updatedEntry.NgaySinh.AddYears(age))
            {
                age--;
            }
            if (age < 18)
            {
                result.Code = 400;
                result.Message = "Người hiến máu chưa đủ 18 tuổi.";
                return result;
            }
            existingEntry.MaDV = updatedEntry.MaDV;
            existingEntry.MaTheTich = updatedEntry.MaTheTich;
            existingEntry.NoiO = updatedEntry.NoiO;
            existingEntry.NgheNghiep = updatedEntry.NgheNghiep;
            existingEntry.ThoiGianDangKy = updatedEntry.ThoiGianDangKy;
            //existingEntry.KetQua = updatedEntry.KetQua != "Không đổi" ? updatedEntry.KetQua : existingEntry.KetQua;
            //existingEntry.ThoiGianHien = updatedEntry.ThoiGianHien;
            var volunteer = _context.tinh_nguyen_vien
                .FirstOrDefault(tnv => tnv.CCCD == existingEntry.CCCD);
            if (volunteer == null)
            {
                result.Code = 400;
                result.Message = "Không tìm thấy tình nguyện viên với ID đã cung cấp.";
                return result;
            }
            volunteer.CCCD = updatedEntry.CCCD;
            volunteer.HoTen = updatedEntry.HoTen;
            volunteer.SoDienThoai = updatedEntry.SoDienThoai;
            volunteer.NgaySinh = updatedEntry.NgaySinh;
            volunteer.GioiTinh = updatedEntry.GioiTinh;
            volunteer.Email = updatedEntry.Email;
            volunteer.MaTinhThanh = updatedEntry.MaTinhThanh;
            volunteer.MaQuanHuyen = updatedEntry.MaQuanHuyen;
            volunteer.MaPhuongXa = updatedEntry.MaPhuongXa;
            _context.SaveChanges();

            result.Code = 400;
            result.Message = "Cập nhật thông tin hiến máu thành công.";
            result.Data = existingEntry;
            return result;
        }
        [Authorize]
        [HttpPut("updateStatus/{id}")]
        public async Task<ActionResult<TemplateResult<object>>> UpdateStatus(ulong id, string ketQua)
        {
            List<string> validKetQua = new List<string> { "Đã hiến", "Chưa hiến", "Từ chối" };
            var result = new TemplateResult<object> { };

            if (!validKetQua.Contains(ketQua))
            {
                result.Code = 400;
                result.Message = "Kết quả phải là Đã hiến, Chưa hiến hoặc Từ chối.";
                return result;
            }

            var existingEntry = await _context.tt_hien_mau
                .FirstOrDefaultAsync(d => d.MaTT == id);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy bản ghi để cập nhật.";
                return result;
            }

            var volunteer = await _context.tinh_nguyen_vien
                .FirstOrDefaultAsync(tnv => tnv.CCCD == existingEntry.CCCD);
            if (volunteer == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy tình nguyện viên với ID đã cung cấp.";
                return result;
            }

            if (existingEntry.KetQua == "Chưa hiến" && ketQua == "Đã hiến")
            {
                existingEntry.ThoiGianHien = DateTime.Now;
                volunteer.SoLanHien++;
            }
            if (existingEntry.KetQua == "Đã hiến" && ketQua == "Chưa hiến")
            {
                existingEntry.ThoiGianHien = null;
                volunteer.SoLanHien--;
            }

            existingEntry.KetQua = ketQua;

            _context.SaveChanges();
            result.Code = 200;
            result.Message = "Cập nhật trạng thái hiến máu thành công.";
            result.Data = existingEntry;
            return result;
        }

        [Authorize]
        [HttpPut("UpdateListStatus")]
        public async Task<ActionResult<TemplateResult<object>>> UpdateListStatus(string ketQua, [FromBody] List<ulong> ids)
        {
            List<string> validKetQua = new List<string> { "Đã hiến", "Chưa hiến", "Từ chối" };
            var result = new TemplateResult<object> { };

            if (!validKetQua.Contains(ketQua))
            {
                result.Code = 400;
                result.Message = "Kết quả phải là Đã hiến, Chưa hiến hoặc Từ chối.";
                return result;
            }

            if (ids == null || ids.Count == 0)
            {
                result.Code = 400;
                result.Message = "Danh sách ID không được để trống.";
                return result;
            }

            foreach (var id in ids)
            {
                var existingEntry = await _context.tt_hien_mau
                .FirstOrDefaultAsync(d => d.MaTT == id);

                if (existingEntry == null)
                {
                    result.Code = 404;
                    result.Message = $"Không tìm thấy bản ghi có {id} để cập nhật.";
                    return result;
                }

                var volunteer = await _context.tinh_nguyen_vien
                    .FirstOrDefaultAsync(tnv => tnv.CCCD == existingEntry.CCCD);
                if (volunteer == null)
                {
                    result.Code = 404;
                    result.Message = "Không tìm thấy tình nguyện viên với ID đã cung cấp.";
                    return result;
                }

                if (existingEntry.KetQua == "Chưa hiến" && ketQua == "Đã hiến")
                {
                    existingEntry.ThoiGianHien = DateTime.Now;
                    volunteer.SoLanHien++;
                }
                if (existingEntry.KetQua == "Đã hiến" && ketQua == "Chưa hiến")
                {
                    existingEntry.ThoiGianHien = null;
                    volunteer.SoLanHien--;
                }

                existingEntry.KetQua = ketQua;
            }

            _context.SaveChanges();
            result.Code = 200;
            result.Message = "Cập nhật trạng thái hiến máu thành công.";
            return result;
        }

        // POST: api/TTHienMau
        [HttpPost]
        public async Task<ActionResult<TemplateResult<object>>> CreateTTHienMau(TTHienMau ttHienMau)
        {
            var result = new TemplateResult<object> { };
            if (!ModelState.IsValid)
            {
                result.Data = 400;
                result.Message = ModelState.ToString();
                //return BadRequest(ModelState); 
                return result;
            }

            var checkTnvId = await _context.tinh_nguyen_vien.FirstOrDefaultAsync(tnv => tnv.CCCD == ttHienMau.CCCD);
            if (checkTnvId == null) return Ok(new TemplateResult<object> { Code = 400, Message = $"Tình nguyện viên id = {ttHienMau.CCCD} chưa được tạo." });

            var checkTheTichId = await _context.the_tich_mau_hien.FirstOrDefaultAsync(tt => tt.MaTheTich == ttHienMau.MaTheTich);
            if (checkTheTichId == null) return Ok(new TemplateResult<object> { Code = 400, Message = $"Thể tích hiến máu id = {ttHienMau.MaTheTich} chưa được tạo." });

            var checkDotHienMauId = await _context.dot_hien_mau.FirstOrDefaultAsync(d => d.MaDot == ttHienMau.MaDot);
            if (checkDotHienMauId == null) return Ok(new TemplateResult<object> { Code = 400, Message = $"Đợt hiến máu id = {ttHienMau.MaDot} chưa được tạo." });

            var checkCoQuanId = await _context.don_vi.FirstOrDefaultAsync(dv => dv.MaDV == ttHienMau.MaDV);
            if (checkCoQuanId == null) return Ok(new TemplateResult<object> { Code = 400, Message = $"Cơ quan id = {ttHienMau.MaDV} chưa được tạo." });

            var existingEntry = await _context.tt_hien_mau
                .FirstOrDefaultAsync(ds => ds.MaDot == ttHienMau.MaDot && ds.CCCD == ttHienMau.CCCD);

            if (existingEntry != null)
            {
                result.Code = 400;
                result.Message = "Bạn đã đăng ký đợt hiến máu này rồi, không thể đăng ký thêm.";
                return result;
            }

            _context.tt_hien_mau.Add(ttHienMau);
            await _context.SaveChangesAsync();

            result.Code = 200;
            result.Message = "Đăng ký hiến máu thành công.";
            result.Data = new { id = ttHienMau.MaTT };
            return result;
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult<PaginatedResult<TTHienMauDto>>> Search(
            string string_tim_kiem = "Nội dung tìm kiếm",
            string string_ketQua = "Nội dung tìm kiếm",
            ulong MaDot = 0,
            int pageSize = 10,
            int currentPage = 1)
        {
            var query = from dsHM in _context.tt_hien_mau
                        join dotHM in _context.dot_hien_mau.AsNoTracking() on dsHM.MaDot equals dotHM.MaDot
                        join tnv in _context.tinh_nguyen_vien.AsNoTracking() on dsHM.CCCD equals tnv.CCCD
                        join tt in _context.the_tich_mau_hien.AsNoTracking() on dsHM.MaTheTich equals tt.MaTheTich
                        join dv in _context.don_vi.AsNoTracking() on dsHM.MaDV equals dv.MaDV
                        join province in _context.Provinces.AsNoTracking() on tnv.MaTinhThanh equals province.ProvinceId
                        join district in _context.Districts.AsNoTracking() on tnv.MaQuanHuyen equals district.DistrictId
                        join ward in _context.Wards.AsNoTracking() on tnv.MaPhuongXa equals ward.WardId
                        select new TTHienMauDto
                        {
                            MaTT = dsHM.MaTT,
                            HoTen = tnv.HoTen,
                            NgaySinh = tnv.NgaySinh,
                            GioiTinh = tnv.GioiTinh,
                            NgheNghiep = dsHM.NgheNghiep,
                            CCCD = tnv.CCCD,
                            SoDienThoai = tnv.SoDienThoai,
                            Email = tnv.Email,
                            NoiO = dsHM.NoiO,
                            SoLanHien = tnv.SoLanHien,
                            KetQua = dsHM.KetQua,
                            TinhThanh = province.Name,
                            QuanHuyen = district.Name,
                            PhuongXa = ward.Name,
                            MaTinhThanh = tnv.MaTinhThanh,
                            MaQuanHuyen = tnv.MaQuanHuyen,
                            MaPhuongXa = tnv.MaPhuongXa,
                            MaDot = dsHM.MaDot,
                            TenDot = dotHM.TenDot,
                            MaDV = dsHM.MaDV,
                            TenDV = dv.TenDV,
                            MaTheTich = dsHM.MaTheTich,
                            TheTich = tt.TheTich,
                            ThoiGianDangKy = dsHM.ThoiGianDangKy,
                            ThoiGianHien = dsHM.ThoiGianHien
                        };


            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.HoTen.Contains(string_tim_kiem) ||
                                         q.CCCD.Contains(string_tim_kiem) ||
                                         q.SoDienThoai.Contains(string_tim_kiem));
            }
            if (!string.IsNullOrEmpty(string_ketQua) && string_ketQua != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.KetQua == string_ketQua);
            }
            if (MaDot > 0)
            {
                query = query.Where(q => q.MaDot == MaDot);
            }

            var totalCount = await query.CountAsync();
            var result = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedResult = new PaginatedResult<TTHienMauDto>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = result
            };
            return Ok(new TemplateResult<PaginatedResult<TTHienMauDto>>
            {
                Code = 200,
                Message = "Tìm kiếm thành công",
                Data = paginatedResult
            });
        }

        [Authorize]
        [HttpGet("dsHMTheoDot")]
        public async Task<ActionResult<TemplateResult<object>>> DsHMTheoDot(int year)
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
        [HttpGet("dsHMTheoThang")]
        public async Task<ActionResult<TemplateResult<object>>> DsHMTheoThang(int year)
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
