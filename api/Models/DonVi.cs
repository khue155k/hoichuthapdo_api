using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class DonVi
    {
        [Key]
        public ulong MaDV { get; set; }
        public string TenDV { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
    }
}