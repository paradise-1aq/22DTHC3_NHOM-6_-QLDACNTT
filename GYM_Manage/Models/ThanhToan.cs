using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GYM_Manage.Models
{
    public class ThanhToan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaThanhToan { get; set; }

        [ForeignKey("ThanhVien")]
        public int MaThanhVien { get; set; }
        public ThanhVien ThanhVien { get; set; }

        [ForeignKey("DangKyGoiTap")]
        public int MaDangKyGoiTap { get; set; }
        public DangKyGoiTap DangKyGoiTap { get; set; }

        [Required]
        public decimal SoTien { get; set; }
        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        [Required]
        public string PhuongThucThanhToan { get; set; }

        [Required]
        public string TrangThai { get; set; } = "ThanhCong";

    }
}
