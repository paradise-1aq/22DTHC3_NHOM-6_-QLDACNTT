using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace GYM_Manage.Controllers.Admin
{
    [Area("Admin")]
    public class QL_GoiTapController : Controller
    {
        
        private readonly GYM_DBcontext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public QL_GoiTapController(GYM_DBcontext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/GoiTap
        public async Task<IActionResult> Index()
        {
            // Check if there are any packages in the database
            if (!await _context.GoiTaps.AnyAsync())
            {
                // Create 10 sample packages if none exist
                var sampleGoiTaps = new List<GoiTap>
                {
                    new GoiTap { TenGoiTap = "Gói Cơ Bản", MoTa = "Gói tập phù hợp cho người mới bắt đầu", ThoiHan = 30, GiaTien = 500000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_coban.jpg" },
                    new GoiTap { TenGoiTap = "Gói Nâng Cao", MoTa = "Gói tập cho người đã có kinh nghiệm", ThoiHan = 30, GiaTien = 800000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_nangcao.jpg" },
                    new GoiTap { TenGoiTap = "Gói VIP", MoTa = "Gói tập cao cấp với huấn luyện viên riêng", ThoiHan = 30, GiaTien = 1500000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_vip.jpg" },
                    new GoiTap { TenGoiTap = "Gói 3 Tháng", MoTa = "Gói tập 3 tháng tiết kiệm", ThoiHan = 90, GiaTien = 1200000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_3thang.jpg" },
                    new GoiTap { TenGoiTap = "Gói 6 Tháng", MoTa = "Gói tập 6 tháng tiết kiệm", ThoiHan = 180, GiaTien = 2000000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_6thang.jpg" },
                    new GoiTap { TenGoiTap = "Gói 1 Năm", MoTa = "Gói tập 1 năm tiết kiệm", ThoiHan = 365, GiaTien = 3500000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_1nam.jpg" },
                    new GoiTap { TenGoiTap = "Gói Yoga", MoTa = "Gói tập yoga chuyên nghiệp", ThoiHan = 30, GiaTien = 700000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_yoga.jpg" },
                    new GoiTap { TenGoiTap = "Gói Cardio", MoTa = "Gói tập cardio giảm cân", ThoiHan = 30, GiaTien = 600000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_cardio.jpg" },
                    new GoiTap { TenGoiTap = "Gói Thể Hình", MoTa = "Gói tập thể hình chuyên sâu", ThoiHan = 30, GiaTien = 900000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_thehinh.jpg" },
                    new GoiTap { TenGoiTap = "Gói Đôi", MoTa = "Gói tập dành cho cặp đôi", ThoiHan = 30, GiaTien = 1000000, TrangThai = "HoatDong", AnhDemo = "/images/goiTap/goitap_doi.jpg" }
                };

                await _context.GoiTaps.AddRangeAsync(sampleGoiTaps);
                await _context.SaveChangesAsync();
            }

            var goiTaps = await _context.GoiTaps.ToListAsync();
            return View("~/Views/Admin/QL_GoiTap/Index.cshtml", goiTaps);
        }

        // GET: Admin/GoiTap/Search
        public async Task<IActionResult> Search(string keyword)
        {
            var goiTaps = await _context.GoiTaps.Where(g => g.TenGoiTap.Contains(keyword)).ToListAsync();
            if (goiTaps.Count == 0)
            {
                TempData["ErrorMessage"] = "Chúng tôi không tìm thấy gói tập nào phù hợp!";
                
                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = $"Chúng tôi tìm thấy {goiTaps.Count} gói tập phù hợp!";
            return View("~/Views/Admin/QL_GoiTap/Index.cshtml", goiTaps);
        }

        // GET: Admin/GoiTap/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var goiTap = await _context.GoiTaps
                .FirstOrDefaultAsync(m => m.MaGoiTap == id);
            
            if (goiTap == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/QL_GoiTap/Details.cshtml", goiTap);
        }

        // GET: Admin/GoiTap/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/QL_GoiTap/CreateOrEdit.cshtml", new GoiTap());
        }

        // POST: Admin/GoiTap/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenGoiTap,MoTa,ThoiHan,GiaTien,TrangThai")] GoiTap goiTap, IFormFile anhDemo)
        {
            ModelState.Remove("AnhDemo");
            
            if (ModelState.IsValid)
            {
                // Xử lý hình ảnh nếu có
                if (anhDemo != null && anhDemo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "goiTap");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + anhDemo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Đảm bảo thư mục tồn tại
                    Directory.CreateDirectory(uploadsFolder);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await anhDemo.CopyToAsync(fileStream);
                    }

                    goiTap.AnhDemo = "/images/goiTap/" + uniqueFileName;
                }
                
                _context.Add(goiTap);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/QL_GoiTap/CreateOrEdit.cshtml", goiTap);
        }

        // GET: Admin/GoiTap/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var goiTap = await _context.GoiTaps.FindAsync(id);
            if (goiTap == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/QL_GoiTap/CreateOrEdit.cshtml", goiTap);
        }

        // POST: Admin/GoiTap/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaGoiTap,TenGoiTap,MoTa,ThoiHan,GiaTien,TrangThai")] GoiTap goiTap, IFormFile anhDemo)
        {
            if (id != goiTap.MaGoiTap)
            {
                return NotFound();
            }

            ModelState.Remove("AnhDemo");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy gói tập hiện tại từ database để lấy đường dẫn ảnh cũ
                    var existingGoiTap = await _context.GoiTaps.AsNoTracking().FirstOrDefaultAsync(g => g.MaGoiTap == id);
                    if (existingGoiTap == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy gói tập!";
                        return RedirectToAction(nameof(Index));
                    }

                    // Xử lý hình ảnh nếu có
                    if (anhDemo != null && anhDemo.Length > 0)
                    {
                        // Xóa hình ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingGoiTap.AnhDemo))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingGoiTap.AnhDemo.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu hình ảnh mới
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "goiTap");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + anhDemo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Đảm bảo thư mục tồn tại
                        Directory.CreateDirectory(uploadsFolder);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await anhDemo.CopyToAsync(fileStream);
                        }

                        goiTap.AnhDemo = "/images/goiTap/" + uniqueFileName;
                    }
                    else
                    {
                        // Nếu không upload ảnh mới, giữ nguyên ảnh cũ
                        goiTap.AnhDemo = existingGoiTap.AnhDemo;
                    }

                    _context.Update(goiTap);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GoiTapExists(goiTap.MaGoiTap))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy gói tập nào phù hợp!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View("~/Views/Admin/QL_GoiTap/CreateOrEdit.cshtml", goiTap);
        }

        // GET: Admin/GoiTap/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var goiTap = await _context.GoiTaps.FindAsync(id);
            if (goiTap != null)
            {
                // Xóa hình ảnh nếu có
                if (!string.IsNullOrEmpty(goiTap.AnhDemo))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, goiTap.AnhDemo.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.GoiTaps.Remove(goiTap);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa thất bại không tìm thấy gói tập nào phù hợp!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GoiTapExists(int id)
        {
            return _context.GoiTaps.Any(e => e.MaGoiTap == id);
        }
    }
}
