using API.Data;
using API.Models;
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
            ngayHomNay = new DateTime(2025, 6, 6);

            var ngayCanNhacNho = ngayHomNay.AddDays(1).Date;

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
                    ThongBao thongBao = new ThongBao();
                    thongBao.TieuDe = "Nhắc nhở hiến máu";
                    thongBao.NoiDung = $"Ngày mai bạn đã đăng ký hiến máu tại {hienMau.DotHienMau.DiaDiem ?? "địa điểm đã đăng ký"}.";
                    thongBao.ThoiGianGui = DateTime.Now;
                    _context.thong_bao.Add(thongBao);
                    _context.SaveChanges();

                    _context.thong_bao_TNV.Add(new ThongBao_TinhNguyenVien
                    {
                        MaTB = thongBao.MaTB,
                        CCCD = hienMau.CCCD,
                    });

                    _context.SaveChanges();

                    await _oneSignalService.SendNotificationList(
                        thongBao.TieuDe,
                        thongBao.NoiDung,
                        new List<string> { userId }
                    );
                }
            }

            return  danhSachNhacNho.Count;
        }

        public async Task<int> ChucMungSinhNhat()
        {
            var today = DateTime.Today;
            today = new DateTime(2025, 6, 6);

            var danhSachChucMung = await _context.tinh_nguyen_vien
                .Where(x => x.OneSiginal_ID != null &&
                            x.NgaySinh.Month == today.Month &&
                            x.NgaySinh.Day == today.Day)
                .ToListAsync();

            foreach (var tnv in danhSachChucMung)
            {
                var userId = tnv.OneSiginal_ID;

                if (!string.IsNullOrEmpty(userId))
                {
                    ThongBao thongBao = new ThongBao();
                    thongBao.TieuDe = "Chúc mừng sinh nhật!";
                    thongBao.NoiDung = $"Hội chữ thập đỏ Hà Nam chúc bạn {tnv.HoTen} sinh nhật vui vẻ luôn mạnh khỏe, hạnh phúc. Mong bạn hãy tích lực tham gia hiến máu góp phần cứu nhiều người hơn nữa!";
                    thongBao.ThoiGianGui = DateTime.Now;
                    _context.thong_bao.Add(thongBao);
                    _context.SaveChanges();

                    _context.thong_bao_TNV.Add(new ThongBao_TinhNguyenVien
                    {
                        MaTB = thongBao.MaTB,
                        CCCD = tnv.CCCD,
                    });
                    _context.SaveChanges();

                    await _oneSignalService.SendNotificationList(
                        thongBao.TieuDe,
                        thongBao.NoiDung,
                        new List<string> { userId }
                    );
                }
            }

            return danhSachChucMung.Count;
        }

    }
}
