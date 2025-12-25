using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_Manage.Models
{
    public class NguoiDung
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // IDENTITY(1,1)
        public int MaNguoiDung { get; set; }

        [Required, MaxLength(50)]
        public string TenDangNhap { get; set; }

        [Required]
        public string MatKhau { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(100)]
        public string HoTen { get; set; }

        [Required, MaxLength(20)]
        public string VaiTro { get; set; }

        [Required, MaxLength(20)]
        public string TrangThai { get; set; } = "HoatDong";

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? LanDangNhapCuoi { get; set; }
    }
}
