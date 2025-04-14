using api.Common;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TinhNguyenVienController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TinhNguyenVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/TinhNguyenVien
        [HttpPost("createTNV")]
        public async Task<ActionResult<TemplateResult<object>>> CreateTinhNguyenVien([FromBody] TinhNguyenVien tinhNguyenVien)
        {
            var result = new TemplateResult<object> { };
            if (!ModelState.IsValid)
            {
                //var errors = ModelState.Values.SelectMany(v => v.Errors)
                //                   .Select(e => e.ErrorMessage);
                //return BadRequest(new TemplateResult<object> { Code = 400, Message = string.Join(", ", errors) });
                result.Code = 400;
                result.Message = ModelState.ToString();
                return result;
            }

            var existingVolunteer = _context.tinh_nguyen_vien
                                            .FirstOrDefault(tnv => tnv.CCCD == tinhNguyenVien.CCCD);
            if (existingVolunteer != null)
            {
                result.Code = 200;
                result.Message = "Tình nguyện viên đã được tạo từ trước";
                result.Data = new { id = existingVolunteer.CCCD };
                return result;
            }

            var currentDate = DateTime.Now;
            var age = currentDate.Year - tinhNguyenVien.NgaySinh.Year;

            if (currentDate < tinhNguyenVien.NgaySinh.AddYears(age))
            {
                age--;
            }

            if (age < 18)
            {
                result.Code = 400;
                result.Message = "Người hiến máu chưa đủ 18 tuổi.";
                return result;
            }
            _context.tinh_nguyen_vien.Add(tinhNguyenVien);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Tạo tình nguyện viên thành công";
            result.Data = new { id = tinhNguyenVien.CCCD };
            return result;
        }

        // GET: api/TinhNguyenVien/cccd/{cccd}
        [HttpGet("cccd/{cccd}")]
        public async Task<ActionResult<TemplateResult<TinhNguyenVien>>> GetTinhNguyenVienByCCCD(string CCCD)
        {
            var result = new TemplateResult<TinhNguyenVien> { };

            var tinhNguyenVien = await _context.tinh_nguyen_vien
                                               .FirstOrDefaultAsync(tnv => tnv.CCCD == CCCD);

            if (tinhNguyenVien == null)
            {
                result.Code = 404;
                result.Message = "Không tìm thấy tình nguyện viên với CCCD này.";
                return result;
            }

            result.Code = 200;
            result.Message = "Lấy thông tin thành công.";
            result.Data = tinhNguyenVien;
            return result;
        }

        [Authorize]
        [HttpPut("updateTNV/{CCCD}")]
        public async Task<ActionResult<TemplateResult<TinhNguyenVien>>> UpdateTNV(string CCCD, [FromBody] TinhNguyenVien TNV)
        {
            var result = new TemplateResult<TinhNguyenVien> { };

            var existingEntry = _context.tinh_nguyen_vien.FirstOrDefault(d => d.CCCD == CCCD);

            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy tình nguyện viên hiến máu có id = {CCCD}";
                return result;
            }
            existingEntry.HoTen = TNV.HoTen;
            existingEntry.NgaySinh = TNV.NgaySinh;
            existingEntry.GioiTinh = TNV.GioiTinh;
            existingEntry.SoDienThoai = TNV.SoDienThoai;
            existingEntry.Email = TNV.Email;
            existingEntry.MaTinhThanh = TNV.MaTinhThanh;
            existingEntry.MaQuanHuyen = TNV.MaQuanHuyen;
            existingEntry.MaPhuongXa = TNV.MaPhuongXa;
            existingEntry.SoLanHien = TNV.SoLanHien;
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Sửa tình nguyện viên hiến máu thành công";
            result.Data = existingEntry;
            return result;
        }
        [Authorize]
        [HttpDelete("deleteTNV/{CCCD}")]
        public async Task<ActionResult<TemplateResult<object>>> DeleteTNV(string CCCD)
        {
            var result = new TemplateResult<object> { };
            var existingEntry = _context.tinh_nguyen_vien.FirstOrDefault(d => d.CCCD == CCCD);
            if (existingEntry == null)
            {
                result.Code = 404;
                result.Message = $"Không tìm thấy tình nguyện viên hiến máu có id = {CCCD}";
                return result;
            }

            _context.tinh_nguyen_vien.Remove(existingEntry);
            _context.SaveChanges();

            result.Code = 200;
            result.Message = "Xóa tình nguyện viên hiến máu thành công";
            return result;
        }

        // GET: api/TinhNguyenVien/search
        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<TinhNguyenVien>>>> Search(
            string string_tim_kiem = "Nội dung tìm kiếm",
            int pageSize = 10,
            int currentPage = 1,
            bool? sortBySoLanHien = false)
        {
            var query = _context.tinh_nguyen_vien.AsEnumerable();

            if (!string.IsNullOrEmpty(string_tim_kiem) && string_tim_kiem != "Nội dung tìm kiếm")
            {
                query = query.Where(q => q.HoTen.Contains(string_tim_kiem) ||
                                         q.SoDienThoai.Contains(string_tim_kiem) ||
                                         q.CCCD.Contains(string_tim_kiem) ||
                                         q.Email.Contains(string_tim_kiem));
            }

            var result = new TemplateResult<PaginatedResult<TinhNguyenVien>>();

            if (sortBySoLanHien == true)
            {
                query = query.OrderByDescending(q => q.SoLanHien);
            }

            var totalCount = query.Count();
            var data = query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            var paginatedResult = new PaginatedResult<TinhNguyenVien>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            result.Code = 200;
            result.Message = "Tìm kiếm danh sách tình nguyện viên hiến máu thành công";
            result.Data = paginatedResult;
            return Ok(result);
        }

        [Authorize]
        [HttpGet("getTNVsPaginated")]
        public async Task<ActionResult<TemplateResult<PaginatedResult<TinhNguyenVien>>>> GetAllTNVPaginated(
            int pageSize = 10,
            int currentPage = 1,
            bool? sortBySoLanHien = false)
        {
            var query = _context.tinh_nguyen_vien.AsQueryable();

            if (sortBySoLanHien == true)
            {
                query = query.OrderByDescending(q => q.SoLanHien);
            }

            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedResult = new PaginatedResult<TinhNguyenVien>
            {
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                Items = data
            };

            var result = new TemplateResult<PaginatedResult<TinhNguyenVien>>
            {
                Code = data.Any() ? 200 : 404,
                Message = data.Any()
                    ? "Lấy danh sách tình nguyện viên hiến máu thành công"
                    : "Không tìm thấy nội dung yêu cầu",
                Data = paginatedResult
            };

            return Ok(result);
        }
        [Authorize]
        [HttpGet("ThongKe/{accID}")]
        public IActionResult GetThongKe(ulong accID)
        {
            var cccd = _context.tinh_nguyen_vien
            .Where(t => t.TaiKhoan_ID == accID)
            .Select(t => t.CCCD)
            .FirstOrDefault();

            var thongTinHienList = _context.tt_hien_mau
                .Where(t => t.CCCD == cccd)
                .ToList();

            if (!thongTinHienList.Any())
            {
                return NotFound("Không tìm thấy dữ liệu hiến máu cho CCCD này.");
            }

            int soLanHien = _context.tinh_nguyen_vien
                .Where(t => t.TaiKhoan_ID == accID)
                .Select(t => t.SoLanHien)
                .FirstOrDefault(); ;

            var tongLuongMau = (from t in thongTinHienList
                                join theTich in _context.the_tich_mau_hien
                                on t.MaTheTich equals theTich.MaTheTich
                                where t.KetQua == "Đã hiến"
                                select theTich.TheTich).Sum();

            var lanCuoi = thongTinHienList
                .OrderByDescending(t => t.ThoiGianHien)
                .First().ThoiGianHien;

            var soQua = _context.lich_su_tang_qua
                .Count(q => q.CCCD == cccd);

            string danhHieu = $"Đã hiến {soLanHien} lần";
            if (soLanHien >= 10) danhHieu = "Hiến máu tiêu biểu";
            else if (soLanHien >= 5) danhHieu = "Đã hiến 5 lần";

            return Ok(new
            {
                soLanHien,
                tongLuongMau,
                lanCuoiHien = lanCuoi,
                danhHieu,
                soQuaDaNhan = soQua
            });
        }
        [Authorize]
        [HttpGet("LichSu/{accId}")]
        public async Task<IActionResult> GetLichSuHienMau(ulong accId)
        {
            var cccd = _context.tinh_nguyen_vien
                  .Where(t => t.TaiKhoan_ID == accId)
                  .Select(t => t.CCCD)
                  .FirstOrDefault();

            if (cccd == null) return NotFound("Không tìm thấy người dùng");

            var lichSu = await (from ttHM in _context.tt_hien_mau
                                join thetich in _context.the_tich_mau_hien on ttHM.MaTheTich equals thetich.MaTheTich
                                join dotHM in _context.dot_hien_mau on ttHM.MaDot equals dotHM.MaDot
                                where ttHM.CCCD == cccd
                                orderby ttHM.ThoiGianHien descending
                                select new 
                                {
                                    ThoiGianHien = ttHM.ThoiGianHien,
                                    TheTich = thetich.TheTich,
                                    KetQua = ttHM.KetQua,
                                    TenDot = dotHM.TenDot,   
                                    DiaDiem = dotHM.DiaDiem     
                                }).ToListAsync();

            return Ok(lichSu);
        }
        [Authorize]
        [HttpPut("updateOnesignalID")]
        public async Task<IActionResult> UpdatePlayerId([FromBody] UpdatePlayerIdRequest request)
        {
            var user = await _context.tinh_nguyen_vien
                .FirstOrDefaultAsync(tnv => tnv.TaiKhoan_ID == request.TaiKhoan_ID);

            if (user == null) return NotFound();

            user.OneSiginal_ID = request.OneSiginal_ID;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật PlayerId thành công" });
        }

        public class UpdatePlayerIdRequest
        {
            public ulong TaiKhoan_ID { get; set; }
            public string OneSiginal_ID { get; set; }
        }

    }
}
