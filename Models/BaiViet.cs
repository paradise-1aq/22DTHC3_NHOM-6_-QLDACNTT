using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_Manage.Models
{
    public class BaiViet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaBaiViet { get; set; }

        [Required, MaxLength(200)]
        public string TieuDe { get; set; }

        [Required, MaxLength(500)]
        public string MoTaNgan { get; set; }

        [Required]
        public string NoiDung { get; set; }

        [MaxLength(555)]
        public string HinhAnh { get; set; }

        [Required]
        public DateTime NgayDang { get; set; } = DateTime.Now;

        public DateTime? NgayCapNhat { get; set; }

        [Required]
        public string TrangThai { get; set; } = "HienThi";

        [Required]
        [ForeignKey("NguoiDung")]
        public int IDNguoiTao { get; set; }

        public virtual NguoiDung NguoiTao { get; set; }

       
    }
}
