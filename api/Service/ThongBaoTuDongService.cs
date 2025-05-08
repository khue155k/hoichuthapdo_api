using API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Service
{
    public class ThongBaoTuDongService
    {
        private readonly ApplicationDbContext _context;
        private readonly OneSignalService _oneSignalService;

        public ThongBaoTuDongService(ApplicationDbContext context, OneSignalService oneSignalService)
        {
            _context = context;
            _oneSignalService = oneSignalService;
        }
        public async Task<int> NhacNhoHienMau()
        {
            var ngayHomNay = DateTime.Today;
            var ngayCanNhacNho = ngayHomNay.AddDays(1);

            var danhSachNhacNho = await _context.tt_hien_mau
                .Include(x => x.TinhNguyenVien)
                .Include(d => d.DotHienMau)
                .Where(x => x.TinhNguyenVien.OneSiginal_ID != null && x.ThoiGianDangKy.Date == ngayCanNhacNho)
                .ToListAsync();

            foreach (var hienMau in danhSachNhacNho)
            {
                var userId = hienMau.TinhNguyenVien.OneSiginal_ID; 

                if (!string.IsNullOrEmpty(userId))
                {
                    await _oneSignalService.SendNotificationList(
                        "Nhắc nhở hiến máu",
                        $"Ngày mai bạn đã đăng ký hiến máu tại {hienMau.DotHienMau.DiaDiem ?? "địa điểm đã đăng ký"}.",
                        new List<string> { userId }
                    );
                }
            }

            return  danhSachNhacNho.Count;
        }

        public async Task<int> ChucMungSinhNhat()
        {
            var danhSachChucMung = await _context.tinh_nguyen_vien
                .Where(x => x.OneSiginal_ID != null && x.NgaySinh.Date == DateTime.Today)
                .ToListAsync();

            foreach (var tnv in danhSachChucMung)
            {
                var userId = tnv.OneSiginal_ID;

                if (!string.IsNullOrEmpty(userId))
                {
                    await _oneSignalService.SendNotificationList(
                        "Chúc mừng sinh nhật!",
                        "Hội chữ thập đỏ Hà Nam cảm ơn bạn đã không ngừng cống hiến cho cộng đồng. Chúc bạn luôn mạnh khỏe, hạnh phúc và tiếp tục truyền cảm hứng cho mọi người!.",
                        new List<string> { userId }
                    );
                }
            }

            return danhSachChucMung.Count;
        }

    }
}
