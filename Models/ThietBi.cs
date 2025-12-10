using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_Manage.Models
{
    public class ThietBi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaThietBi { get; set; }

        [Required]
        public string TenThietBi { get; set; }
        public string DanhMuc { get; set; }
        public DateTime? NgayMua { get; set; }
        public DateTime? NgayBaoTriCuoi { get; set; }

        [Required]
        public string TrangThai { get; set; } = "HoatDong";
    }
}
