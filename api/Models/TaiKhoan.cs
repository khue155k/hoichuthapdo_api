using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class TaiKhoan : IdentityUser
    {
        //[Key]
        //public ulong Id { get; set; }
        //[Required]
        //public string Username { get; set; }
        //[Required]
        //public string Password { get; set; }
        //[Required]
        //public string Role { get; set; }
        //public ulong Id { get; set; }
        public string? EmailVerificationCode { get; set; }
        public string? CCCD { get; set; }
        public DateTime? Create_time { get; set; }
        public ICollection<TinhNguyenVien>? TinhNguyenViens { get; set; }
        public ICollection<QuanTriVien>? QuanTriViens { get; set; }
    }
}