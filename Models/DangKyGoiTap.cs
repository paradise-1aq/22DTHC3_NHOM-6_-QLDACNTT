using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GYM_Manage.Models
{
    public class DangKyGoiTap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDangKy { get; set; }

        [ForeignKey("ThanhVien")]
        public int MaThanhVien { get; set; }
        public ThanhVien ThanhVien { get; set; }

        [ForeignKey("GoiTap")]
        public int MaGoiTap { get; set; }
        public GoiTap GoiTap { get; set; }

        [Required]
        public DateTime NgayBatDau { get; set; }

        [Required]
        public DateTime NgayKetThuc { get; set; }

        [Required]
        public string TrangThai { get; set; } = "HoatDong";
    }
}
