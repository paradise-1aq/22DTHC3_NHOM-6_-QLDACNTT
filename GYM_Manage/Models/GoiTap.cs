using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GYM_Manage.Models
{
    public class GoiTap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaGoiTap { get; set; }

 
        [Required]
        public string TenGoiTap { get; set; }
        public string AnhDemo { get; set; }
        public string MoTa { get; set; }

        [Required]
        public int ThoiHan { get; set; }        

        [Required]
        public decimal GiaTien { get; set; }

        [Required]
        public string TrangThai { get; set; } = "HoatDong";
    }
}
