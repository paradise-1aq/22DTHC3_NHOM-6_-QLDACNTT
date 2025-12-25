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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GYM_Manage.Controllers.Admin
{
    [Area("Admin")]
    public class QL_BaiVietController : Controller
    {
        private readonly GYM_DBcontext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public QL_BaiVietController(GYM_DBcontext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/BaiViet
        public async Task<IActionResult> Index()
        {
            var baiViets = await _context.BaiViets
                .Include(b => b.NguoiTao)
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();
            return View("~/Views/Admin/QL_BaiViet/Index.cshtml", baiViets);
        }

        // GET: Admin/BaiViet/Search
        public async Task<IActionResult> Search(string keyword)
        {
            var baiViets = await _context.BaiViets
                .Include(b => b.NguoiTao)
                .Where(b => b.TieuDe.Contains(keyword) || b.MoTaNgan.Contains(keyword))
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            if (baiViets.Count == 0)
            {
                TempData["ErrorMessage"] = "Chúng tôi không tìm thấy bài viết nào phù hợp!";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Chúng tôi tìm thấy {baiViets.Count} bài viết phù hợp!";
            return View("~/Views/Admin/QL_BaiViet/Index.cshtml", baiViets);
        }

        // GET: Admin/BaiViet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiViet = await _context.BaiViets
                .Include(b => b.NguoiTao)
                .FirstOrDefaultAsync(m => m.MaBaiViet == id);

            if (baiViet == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/QL_BaiViet/Details.cshtml", baiViet);
        }

        // GET: Admin/BaiViet/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/QL_BaiViet/CreateOrEdit.cshtml", new BaiViet());
        }

        // POST: Admin/BaiViet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TieuDe,MoTaNgan,NoiDung,HinhAnh,TrangThai")] BaiViet baiViet, IFormFile hinhAnh)
        {
            ModelState.Remove("HinhAnh");
            ModelState.Remove("NguoiTao");
            baiViet.IDNguoiTao = 1;
            
            if (ModelState.IsValid)
            {
                // Xử lý hình ảnh nếu có
                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "baiViet");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + hinhAnh.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Đảm bảo thư mục tồn tại
                    Directory.CreateDirectory(uploadsFolder);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnh.CopyToAsync(fileStream);
                    }

                    baiViet.HinhAnh = "/images/baiViet/" + uniqueFileName;
                }

                // Thiết lập thông tin bài viết
                baiViet.NgayDang = DateTime.Now;
                baiViet.IDNguoiTao = 1; // Thay đổi sau khi có hệ thống đăng nhập


                _context.Add(baiViet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm bài viết thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Thêm bài viết thất bại!";
            return View("~/Views/Admin/QL_BaiViet/CreateOrEdit.cshtml", baiViet);
        }

        // GET: Admin/BaiViet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiViet = await _context.BaiViets.FindAsync(id);
            if (baiViet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài viết!";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/QL_BaiViet/CreateOrEdit.cshtml", baiViet);
        }

        // POST: Admin/BaiViet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBaiViet,TieuDe,MoTaNgan,NoiDung,HinhAnh,NgayDang,TrangThai,IDNguoiTao")] BaiViet baiViet, IFormFile hinhAnh)
        {
            ModelState.Remove("HinhAnh");
            ModelState.Remove("NguoiTao");
            if (id != baiViet.MaBaiViet)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy bài viết hiện tại từ database để lấy đường dẫn ảnh cũ
                    var existingBaiViet = await _context.BaiViets.AsNoTracking().FirstOrDefaultAsync(b => b.MaBaiViet == id);
                    if (existingBaiViet == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy bài viết!";
                        return RedirectToAction(nameof(Index));
                    }

                    // Xử lý hình ảnh nếu có
                    if (hinhAnh != null && hinhAnh.Length > 0)
                    {
                        // Xóa hình ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingBaiViet.HinhAnh))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingBaiViet.HinhAnh.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu hình ảnh mới
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "baiViet");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + hinhAnh.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Đảm bảo thư mục tồn tại
                        Directory.CreateDirectory(uploadsFolder);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await hinhAnh.CopyToAsync(fileStream);
                        }

                        baiViet.HinhAnh = "/images/baiViet/" + uniqueFileName;
                    }
                    else
                    {
                        // Nếu không upload ảnh mới, giữ nguyên ảnh cũ
                        baiViet.HinhAnh = existingBaiViet.HinhAnh;
                    }

                    // Cập nhật thông tin bài viết
                    baiViet.NgayCapNhat = DateTime.Now;

                    _context.Update(baiViet);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaiVietExists(baiViet.MaBaiViet))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy bài viết!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View("~/Views/Admin/QL_BaiViet/CreateOrEdit.cshtml", baiViet);
        }

        // GET: Admin/BaiViet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var baiViet = await _context.BaiViets.FindAsync(id);
            if (baiViet != null)
            {
                // Xóa hình ảnh nếu có
                if (!string.IsNullOrEmpty(baiViet.HinhAnh))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, baiViet.HinhAnh.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.BaiViets.Remove(baiViet);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bài viết thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa thất bại! Không tìm thấy bài viết!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/BaiViet/ChangeStatus/5
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id)
        {
            var baiViet = await _context.BaiViets.FindAsync(id);
            if (baiViet == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài viết!" });
            }

            // Đổi trạng thái
            baiViet.TrangThai = baiViet.TrangThai == "HienThi" ? "An" : "HienThi";
            baiViet.NgayCapNhat = DateTime.Now;

            _context.Update(baiViet);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "Cập nhật trạng thái thành công!", 
                newStatus = baiViet.TrangThai,
                statusText = baiViet.TrangThai == "HienThi" ? "Hiển thị" : "Ẩn"
            });
        }

        private bool BaiVietExists(int id)
        {
            return _context.BaiViets.Any(e => e.MaBaiViet == id);
        }
    }
}
