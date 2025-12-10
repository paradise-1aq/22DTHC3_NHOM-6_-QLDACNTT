using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GYM_Manage.Models
{
    public class HoaDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHoaDon { get; set; }

        [ForeignKey("ThanhVien")]
        public int MaThanhVien { get; set; }
        public ThanhVien ThanhVien { get; set; }

        [ForeignKey("ThanhToan")]
        public int MaThanhToan { get; set; }
        public ThanhToan ThanhToan { get; set; }

        public DateTime NgayHoaDon { get; set; } = DateTime.Now;

        [Required]
        public decimal TongSoTien { get; set; }

        [Required]
        public string TrangThai { get; set; } = "ChuaThanhToan";
    }
}
