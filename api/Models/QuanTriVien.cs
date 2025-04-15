using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class QuanTriVien
    {
        [Key]
        public ulong MaQTV { get; set; }
        public string TenQTV { get; set; }
        public string ChucVu { get; set; }
        public string BoPhan { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string TaiKhoan_ID { get; set; }
        [ForeignKey("TaiKhoan_ID")] public TaiKhoan? TaiKhoan { get; set; }

    }
}
