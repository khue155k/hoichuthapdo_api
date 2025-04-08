namespace API.Models
{
    public class DotHienMau
    {
        public ulong MaDot { get; set; }
        public string TenDot { get; set; }
        public string DiaDiem { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public int DonViMau { get; set; }
    }
}