using System.ComponentModel.DataAnnotations;

namespace API.Models {
    public class TTHienMau {
        [Key]
        [Required(ErrorMessage = "Trường 'Mã thông tin' không được để trống.")]
        public ulong MaTT { get; set; }

        [Required(ErrorMessage = "Trường 'Mã đợt' không được để trống.")]
        public ulong MaDot { get; set; }

        [Required(ErrorMessage = "Trường 'CCCD' không được để trống.")]
        public string CCCD { get; set; }

        [Required(ErrorMessage = "Trường 'Mã thể tích' không được để trống.")]
        public ulong MaTheTich { get; set; }

        [Required(ErrorMessage = "Trường 'Mã đơn vị' không được để trống.")]
        public ulong MaDV { get; set; }

        [Required(ErrorMessage = "Trường 'Nghề nghiệp' không được để trống.")]
        public string NgheNghiep { get; set; }

        [Required(ErrorMessage = "Trường 'Nơi ở' không được để trống.")]
        public string NoiO { get; set; }

        [Required(ErrorMessage = "Trường 'Thời gian đăng ký' không được để trống.")]
        public DateTime ThoiGianDangKy { get; set; }
        public DateTime? ThoiGianHien { get; set; }

        [Required(ErrorMessage = "Trường 'Kết quả' không được để trống.")]
        public string KetQua { get; set; }
    }
}