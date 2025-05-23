﻿using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class QuaTang
    {
        [Key]
        public ulong MaQua { get; set; }
        public string TenQua { get; set; }
        public decimal GiaTri { get; set; }
        public ICollection<LichSuTangQua>? LichSuTangQuas { get; set; }
    }
}
