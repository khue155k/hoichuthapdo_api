public class TTHienMauDto
{
    public ulong MaTT { get; set; }
    public string HoTen { get; set; }
    public DateTime NgaySinh { get; set; }
    public string GioiTinh { get; set; }
    public string NgheNghiep { get; set; }
    public ulong MaDV { get; set; }
    public string TenDV { get; set; }
    public string CCCD { get; set; }
    public string SoDienThoai { get; set; }
    public string Email { get; set; }
    public string NoiO { get; set; }
    public long SoLanHien { get; set; }
    public string KetQua { get; set; }
    public string TinhThanh { get; set; }
    public string QuanHuyen { get; set; }
    public string PhuongXa { get; set; }
    public ulong MaTinhThanh { get; set; }
    public ulong MaQuanHuyen { get; set; }
    public ulong MaPhuongXa { get; set; }
    public ulong MaDot { get; set; }
    public string TenDot { get; set; }
    public ulong MaTheTich { get; set; }
    public int TheTich { get; set; }
    public DateTime ThoiGianDangKy { get; set; }
    public DateTime? ThoiGianHien { get; set; }
}
public class UpdateTTHienMauDto
{
    public string CCCD { get; set; }
    public string HoTen { get; set; }
    public string SoDienThoai { get; set; }
    public DateTime NgaySinh { get; set; }
    public string GioiTinh { get; set; }
    public ulong MaTinhThanh { get; set; }
    public ulong MaQuanHuyen { get; set; }
    public ulong MaPhuongXa { get; set; }
    public string Email { get; set; }
    public long SoLanHien { get; set; }
    public string NoiO { get; set; }
    public string NgheNghiep { get; set; }
    public ulong MaDV { get; set; }
    public DateTime ThoiGianDangKy { get; set; }
    public ulong MaTheTich { get; set; }
}