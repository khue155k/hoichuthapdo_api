using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class ThongBao
    {
        [Key]
        public ulong MaTB { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGianGui { get; set; }
    }
}