using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GYM_Manage.Models
{
    public class ThanhVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaThanhVien { get; set; }

        [ForeignKey("NguoiDung")]
        public int? MaNguoiDung { get; set; }
        public NguoiDung NguoiDung { get; set; }

        public DateTime? NgaySinh { get; set; }

        [MaxLength(10)]
        public string GioiTinh { get; set; }

        [MaxLength(15)]
        public string SoDienThoai { get; set; }

        public string DiaChi { get; set; }
        public ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();

    }
}
