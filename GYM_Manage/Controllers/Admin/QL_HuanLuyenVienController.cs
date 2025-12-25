using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GYM_Manage.Controllers.Admin
{
    [Area("Admin")]
    public class QL_HuanLuyenVienController : Controller
    {
        private readonly GYM_DBcontext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public QL_HuanLuyenVienController(GYM_DBcontext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/HuanLuyenVien
        public async Task<IActionResult> Index()
        {
            // Check if there are any trainers in the database
           

            var huanLuyenViens = await _context.HuanLuyenViens.Include(h => h.NguoiDung).ToListAsync();
            return View("~/Views/Admin/QL_HuanLuyenVien/Index.cshtml", huanLuyenViens);
        }

        // GET: Admin/HuanLuyenVien/Search
        public async Task<IActionResult> Search(string keyword)
        {
            var huanLuyenViens = await _context.HuanLuyenViens
                .Include(h => h.NguoiDung)
                .Where(h => h.NguoiDung.HoTen.Contains(keyword) || h.ChuyenMon.Contains(keyword))
                .ToListAsync();

            if (huanLuyenViens.Count == 0)
            {
                TempData["ErrorMessage"] = "Chúng tôi không tìm thấy huấn luyện viên nào phù hợp!";
                return RedirectToAction(nameof(Index));
            }
            
            TempData["SuccessMessage"] = $"Chúng tôi tìm thấy {huanLuyenViens.Count} huấn luyện viên phù hợp!";
            return View("~/Views/Admin/QL_HuanLuyenVien/Index.cshtml", huanLuyenViens);
        }

        // GET: Admin/HuanLuyenVien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var huanLuyenVien = await _context.HuanLuyenViens
                .Include(h => h.NguoiDung)
                .FirstOrDefaultAsync(m => m.MaHuanLuyenVien == id);
                
            if (huanLuyenVien == null)
            {
                return NotFound();
            }

            return View(huanLuyenVien);
        }

        // GET: Admin/HuanLuyenVien/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/QL_HuanLuyenVien/CreateOrEdit.cshtml", new HuanLuyenVien());
        }

        // POST: Admin/HuanLuyenVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoTen,ChuyenMon,ChungChi,NgayTuyenDung")] HuanLuyenVien huanLuyenVien, IFormFile anhDaiDien, string tenDangNhap, string email, string matKhau)
        {
            ModelState.Remove("AnhDaiDien");
            ModelState.Remove("NguoiDung");
            ModelState.Remove("MaNguoiDung");
            
            if (ModelState.IsValid)
            {
                // Tạo mới người dùng
                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = tenDangNhap,
                    Email = email,
                    MatKhau = matKhau,
                    HoTen = huanLuyenVien.HoTen,
                    VaiTro = "Trainer", // Vai trò huấn luyện viên
                    TrangThai = "HoatDong",
                    NgayTao = DateTime.Now
                };
                
                // Thêm người dùng vào database
                _context.NguoiDungs.Add(nguoiDung);
                await _context.SaveChangesAsync();
                
                // Gán mã người dùng vừa tạo cho huấn luyện viên
                huanLuyenVien.MaNguoiDung = nguoiDung.MaNguoiDung;
                
                // Xử lý ảnh đại diện nếu có
                if (anhDaiDien != null && anhDaiDien.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "huanluyenvien");
                    
                    // Đảm bảo thư mục tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + anhDaiDien.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await anhDaiDien.CopyToAsync(fileStream);
                    }

                    huanLuyenVien.AnhDaiDien = "/images/huanluyenvien/" + uniqueFileName;
                }

                _context.Add(huanLuyenVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm thành công!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Thêm thất bại!";
            return View("~/Views/Admin/QL_HuanLuyenVien/CreateOrEdit.cshtml", huanLuyenVien);
        }

        // GET: Admin/HuanLuyenVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var huanLuyenVien = await _context.HuanLuyenViens.FindAsync(id);
            if (huanLuyenVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy huấn luyện viên nào phù hợp!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/QL_HuanLuyenVien/CreateOrEdit.cshtml", huanLuyenVien);
        }

        // POST: Admin/HuanLuyenVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HoTen,MaHuanLuyenVien,MaNguoiDung,ChuyenMon,ChungChi,NgayTuyenDung,AnhDaiDien")] HuanLuyenVien huanLuyenVien, IFormFile anhDaiDien)
        {
            if (id != huanLuyenVien.MaHuanLuyenVien)
            {
                return NotFound();
            }
            ModelState.Remove("AnhDaiDien");
            ModelState.Remove("NguoiDung");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy huấn luyện viên hiện tại từ database để lấy đường dẫn ảnh cũ
                    var existingHLV = await _context.HuanLuyenViens.AsNoTracking().FirstOrDefaultAsync(h => h.MaHuanLuyenVien == id);
                    if (existingHLV == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy huấn luyện viên!";
                        return RedirectToAction(nameof(Index));
                    }

                    // Xử lý ảnh đại diện nếu có
                    if (anhDaiDien != null && anhDaiDien.Length > 0)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingHLV.AnhDaiDien))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingHLV.AnhDaiDien.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu ảnh mới
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "huanluyenvien");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + anhDaiDien.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Đảm bảo thư mục tồn tại
                        Directory.CreateDirectory(uploadsFolder);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await anhDaiDien.CopyToAsync(fileStream);
                        }

                        huanLuyenVien.AnhDaiDien = "/images/huanluyenvien/" + uniqueFileName;
                    }
                    else
                    {
                        // Nếu không upload ảnh mới, giữ nguyên ảnh cũ
                        huanLuyenVien.AnhDaiDien = existingHLV.AnhDaiDien;
                    }

                    _context.Update(huanLuyenVien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HuanLuyenVienExists(huanLuyenVien.MaHuanLuyenVien))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy huấn luyện viên nào phù hợp!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View("~/Views/Admin/QL_HuanLuyenVien/CreateOrEdit.cshtml", huanLuyenVien);
        }

        // GET: Admin/HuanLuyenVien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var huanLuyenVien = await _context.HuanLuyenViens.FindAsync(id);
            if (huanLuyenVien != null)
            {
                // Xóa ảnh đại diện nếu có
                if (!string.IsNullOrEmpty(huanLuyenVien.AnhDaiDien))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, huanLuyenVien.AnhDaiDien.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.HuanLuyenViens.Remove(huanLuyenVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa thất bại không tìm thấy huấn luyện viên nào phù hợp!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool HuanLuyenVienExists(int id)
        {
            return _context.HuanLuyenViens.Any(e => e.MaHuanLuyenVien == id);
        }
    }
}
