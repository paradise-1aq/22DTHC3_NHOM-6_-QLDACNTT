using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GYM_Manage.Models
{
    public class HuanLuyenVien
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaHuanLuyenVien { get; set; }

        [ForeignKey("NguoiDung")]
        public int? MaNguoiDung { get; set; }

        public string HoTen { get; set; }
        public NguoiDung NguoiDung { get; set; }
        public string AnhDaiDien { get; set; }
        public string ChuyenMon { get; set; }
        public string ChungChi { get; set; }
        public DateTime? NgayTuyenDung { get; set; }
    }
}
