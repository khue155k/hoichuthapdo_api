using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class LichSuTangQua
    {
        [ForeignKey("CCCD")] public string CCCD { get; set; }
        [ForeignKey("MaQua")] public ulong MaQua { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGianGui { get; set; }

        [ForeignKey("CCCD")] public TinhNguyenVien? TinhNguyenVien { get; set; }
        [ForeignKey("MaQua")] public QuaTang? QuaTang { get; set; }
    }
}