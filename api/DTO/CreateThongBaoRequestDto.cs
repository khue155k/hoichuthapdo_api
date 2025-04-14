using API.Models;

public class CreateThongBaoRequestDto
{
    public ThongBao ThongBao { get; set; }
    public List<TinhNguyenVien> DSTinhNguyenVien { get; set; }
}