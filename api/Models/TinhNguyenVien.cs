using System.ComponentModel.DataAnnotations;
using Telegram.BotAPI.AvailableTypes;

namespace API.Models
{
    public class TinhNguyenVien
    {
        [Key]
        [Required(ErrorMessage = "Trường 'CCCD' không được để trống.")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Trường 'CCCD' phải là số và có 12 chữ số.")]
        public string CCCD { get; set; }

        [Required(ErrorMessage = "Trường 'Họ tên' không được để trống.")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Trường 'Ngày sinh' không được để trống.")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Trường 'Giới tính' không được để trống.")]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Trường 'Số điện thoại' không được để trống.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Trường 'SDT' phải là số và có 10 chữ số.")]
        public string SoDienThoai { get; set; }
        public string? Email { get; set; }

        [Required(ErrorMessage = "Trường 'Tỉnh thành code' không được để trống.")]
        public ulong MaTinhThanh { get; set; }

        [Required(ErrorMessage = "Trường 'Quận huyện code' không được để trống.")]
        public ulong MaQuanHuyen { get; set; }

        [Required(ErrorMessage = "Trường 'Phường xã code' không được để trống.")]
        public ulong MaPhuongXa { get; set; }

        [Required(ErrorMessage = "Trường 'Số lần hiến' không được để trống.")]
        [Range(0, ulong.MaxValue, ErrorMessage = "Trường 'Số lần hiến máu' phải là số không âm.")]
        public int SoLanHien { get; set; }
        public ulong? TaiKhoan_ID { get; set; }
        public string? OneSiginal_ID { get; set; }

    }
}
