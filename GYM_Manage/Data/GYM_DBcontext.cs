using GYM_Manage.Models;
using Microsoft.EntityFrameworkCore;

namespace GYM_Manage.Data
{
    public class GYM_DBcontext : DbContext
    {

        public GYM_DBcontext(DbContextOptions<GYM_DBcontext> options) : base(options) { }

        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<ThanhVien> ThanhViens { get; set; }
        public DbSet<HuanLuyenVien> HuanLuyenViens { get; set; }
        public DbSet<GoiTap> GoiTaps { get; set; }
        public DbSet<DangKyGoiTap> DangKyGoiTaps { get; set; }
        public DbSet<ThietBi> ThietBis { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<BaiViet> BaiViets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Đảm bảo tên đăng nhập là duy nhất
            modelBuilder.Entity<NguoiDung>()
                .HasIndex(u => u.TenDangNhap)
                .IsUnique();

            // Đảm bảo số điện thoại của thành viên là duy nhất
            modelBuilder.Entity<ThanhVien>()
                .HasIndex(tv => tv.SoDienThoai)
                .IsUnique();

            // Định nghĩa kiểu dữ liệu decimal cho các cột liên quan đến tiền tệ
            modelBuilder.Entity<GoiTap>()
                .Property(g => g.GiaTien)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<HoaDon>()
                .Property(h => h.TongSoTien)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ThanhToan>()
                .Property(t => t.SoTien)
                .HasColumnType("decimal(18,2)");

            // Cấu hình quan hệ giữa ThanhToan và ThanhVien để tránh lỗi multiple cascade paths
            modelBuilder.Entity<ThanhToan>()
       .HasOne(t => t.ThanhVien)
       .WithMany(tv => tv.ThanhToans) // 🔹 Định nghĩa quan hệ ngược lại
       .HasForeignKey(t => t.MaThanhVien)
       .OnDelete(DeleteBehavior.NoAction);


            // Cấu hình quan hệ giữa ThanhToan và DangKyGoiTap
            modelBuilder.Entity<ThanhToan>()
                .HasOne(t => t.DangKyGoiTap)
                .WithMany()
                .HasForeignKey(t => t.MaDangKyGoiTap)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BaiViet>()
                .HasOne(b => b.NguoiTao)
                .WithMany()
                .HasForeignKey(b => b.IDNguoiTao)
                .OnDelete(DeleteBehavior.Restrict);  // 🔹 Hạn chế xóa nếu có liên kết
        }
    }
}
