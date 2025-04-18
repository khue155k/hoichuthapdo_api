using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data
{
    public class ApplicationDbContext : IdentityDbContext<TaiKhoan>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<DonVi> don_vi { get; set; }
        public DbSet<DotHienMau> dot_hien_mau { get; set; }
        public DbSet<LichSuTangQua> lich_su_tang_qua { get; set; }
        public DbSet<QuanTriVien> quan_tri_vien { get; set; }
        public DbSet<QuaTang> qua_tang { get; set; }
        public DbSet<TaiKhoan> tai_khoan { get; set; }
        public DbSet<TheTichMauHien> the_tich_mau_hien { get; set; }
        public DbSet<TinhNguyenVien> tinh_nguyen_vien { get; set; }
        public DbSet<TTHienMau> tt_hien_mau { get; set; }
        public DbSet<ThongBao_TinhNguyenVien> thong_bao_TNV { get; set; }
        public DbSet<ThongBao> thong_bao { get; set; }

        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("identity");

            modelBuilder.Entity<LichSuTangQua>()
            .HasKey(x => new { x.CCCD, x.MaQua });

            modelBuilder.Entity<ThongBao_TinhNguyenVien>()
                .HasKey(x => new { x.MaTB, x.CCCD });

        }
    }
}
