using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GYM_Manage.Controllers
{
    public class AuthController : Controller
    {
        private readonly GYM_DBcontext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(GYM_DBcontext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string tenDangNhap, string matKhau)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên đăng nhập và mật khẩu";
                return View();
            }

            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap && u.TrangThai == "HoatDong");

            if (user == null || matKhau != user.MatKhau)
            {
                TempData["ErrorMessage"] = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View();
            }

            // Cập nhật thời gian đăng nhập cuối
            user.LanDangNhapCuoi = DateTime.Now;
            await _context.SaveChangesAsync();
            
            // Lưu thông tin người dùng vào Session
            HttpContext.Session.SetString("UserName", user.HoTen);
            HttpContext.Session.SetString("UserRole", user.VaiTro); // Lưu vai trò (nếu cần)

            TempData["SuccessMessage"] = $"Chào mừng {user.HoTen} đã đăng nhập thành công!";

            // Kiểm tra vai trò người dùng để chuyển hướng
            if (user.VaiTro == "Admin" || user.VaiTro == "Staff")
            {
                if (user.VaiTro == "Admin") { return RedirectToAction("Index", "Dashboard", new { area = "Admin" }); }
                if (user.VaiTro == "Staff") { return RedirectToAction("Index", "QL_ThanhVien", new { area = "Admin" }); }
            }
            
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(NguoiDung model)
        {
            ModelState.Remove("VaiTro"); // Không cho phép người dùng tự chọn vai trò
            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                var existingUser = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap);

                if (existingUser != null)
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                existingUser = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Không mã hóa mật khẩu, lưu trực tiếp
                model.VaiTro = "Member"; // Mặc định là thành viên
                model.NgayTao = DateTime.Now;
                model.TrangThai = "HoatDong";

                _context.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Người dùng {model.TenDangNhap} đã đăng ký thành công!";

                return RedirectToAction(nameof(Login));
            }

            TempData["ErrorMessage"] = "Đăng ký thất bại, vui lòng kiểm tra lại thông tin!";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Lưu vai trò trước khi đăng xuất để xác định trang chuyển hướng
            string userRole = HttpContext.Session.GetString("UserRole");
            
            // Xóa session và đăng xuất
            HttpContext.Session.Clear();
            
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            
            // Nếu người dùng là Admin hoặc Staff, chuyển về trang đăng nhập
            if (userRole == "Admin" || userRole == "Staff")
            {
                return RedirectToAction("Login", "Auth");
            }
            
            return RedirectToAction("Index", "Home");
        }
    }
}
