using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Models
{
    public class ThongBao_TinhNguyenVien
    {
        [ForeignKey("MaTB")] public ulong MaTB { get; set; }
        [ForeignKey("CCCD")] public string CCCD { get; set; }

        [JsonIgnore]
        [ForeignKey("MaTB")] public ThongBao? ThongBao { get; set; }
        [JsonIgnore]
        [ForeignKey("CCCD")] public TinhNguyenVien? TinhNguyenVien { get; set; }
    }
}