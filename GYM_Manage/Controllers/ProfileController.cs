using System;
using System.Linq;
using System.Threading.Tasks;
using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace GYM_Manage.Controllers
{
    public class ProfileController : Controller
    {
        private readonly GYM_DBcontext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(GYM_DBcontext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra người dùng đã đăng nhập chưa
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Lấy tên đăng nhập từ session
            string userName = HttpContext.Session.GetString("UserName");

            // Tìm thông tin người dùng
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.HoTen == userName);

            if (nguoiDung == null)
            {
                return NotFound();
            }

            // Tìm thông tin thành viên (nếu có)
            var thanhVien = await _context.ThanhViens
                .FirstOrDefaultAsync(t => t.MaNguoiDung == nguoiDung.MaNguoiDung);

            // Lấy thông tin gói tập đã đăng ký
            var dangKyGoiTap = await _context.DangKyGoiTaps
                .Where(d => d.MaThanhVien == nguoiDung.MaNguoiDung)
                .Include(d => d.GoiTap)
                .OrderByDescending(d => d.NgayBatDau)
                .ToListAsync();

            // Lấy lịch sử thanh toán
            var lichSuThanhToan = await _context.ThanhToans
                .Where(t => t.MaThanhVien == nguoiDung.MaNguoiDung)
                .OrderByDescending(t => t.NgayThanhToan)
                .ToListAsync();

            // Tạo view model để truyền dữ liệu đến view
            var viewModel = new ProfileViewModel
            {
                NguoiDung = nguoiDung,
                ThanhVien = thanhVien,
                DangKyGoiTaps = dangKyGoiTap,
                LichSuThanhToans = lichSuThanhToan
            };

            return View(viewModel);
        }

        

       

      
    }
}

namespace GYM_Manage.Models
{
    public class ProfileViewModel
    {
        public NguoiDung NguoiDung { get; set; }
        public ThanhVien ThanhVien { get; set; }
        public List<DangKyGoiTap> DangKyGoiTaps { get; set; }
        public List<ThanhToan> LichSuThanhToans { get; set; }
    }
}
