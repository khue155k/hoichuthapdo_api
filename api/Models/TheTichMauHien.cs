using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class TheTichMauHien
    {
        [Key]
        public ulong MaTheTich { get; set; }
        public int TheTich { get; set; }
        public ICollection<TTHienMau>? TTHienMaus { get; set; }

    }
}