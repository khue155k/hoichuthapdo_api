using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class TaiKhoan
    {
        [Key]
        public ulong ID { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        public DateTime Create_time { get; set; }
    }
}
