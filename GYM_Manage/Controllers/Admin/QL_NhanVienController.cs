using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GYM_Manage.Controllers.Admin
{
    [Area("Admin")]
    public class QL_NhanVienController : Controller
    {
        private readonly GYM_DBcontext _context;

        public QL_NhanVienController(GYM_DBcontext context)
        {
            _context = context;
        }

        // GET: Admin/NhanVien
        public async Task<IActionResult> Index()
        {
            // Check if there are any employees in the database
            

            var nhanViens = await _context.NguoiDungs.Where(n => n.VaiTro == "Staff").ToListAsync();
            return View("~/Views/Admin/QL_NhanVien/Index.cshtml", nhanViens);
        }

        // GET: Admin/NhanVien/Search
        public async Task<IActionResult> Search(string keyword)
        {
            var nhanViens = await _context.NguoiDungs
                .Where(n => n.VaiTro == "Staff" && 
                      (n.HoTen.Contains(keyword) || 
                       n.Email.Contains(keyword) || 
                       n.TenDangNhap.Contains(keyword)))
                .ToListAsync();

            if (nhanViens.Count == 0)
            {
                TempData["ErrorMessage"] = "Chúng tôi không tìm thấy nhân viên nào phù hợp!";
                return RedirectToAction(nameof(Index));
            }
            
            TempData["SuccessMessage"] = $"Chúng tôi tìm thấy {nhanViens.Count} nhân viên phù hợp!";
            return View("~/Views/Admin/QL_NhanVien/Index.cshtml", nhanViens);
        }

        // GET: Admin/NhanVien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NguoiDungs
                .FirstOrDefaultAsync(m => m.MaNguoiDung == id && m.VaiTro == "Staff");
                
            if (nhanVien == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/QL_NhanVien/Details.cshtml", nhanVien);
        }

        // GET: Admin/NhanVien/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/QL_NhanVien/CreateOrEdit.cshtml", new NguoiDung { VaiTro = "Staff", TrangThai = "HoatDong" });
        }

        // POST: Admin/NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDangNhap,MatKhau,Email,HoTen,VaiTro,TrangThai")] NguoiDung nhanVien)
        {
            if (ModelState.IsValid)
            {
                // Ensure the role is set to Staff
                nhanVien.VaiTro = "Staff";
                
                // Set creation date
                nhanVien.NgayTao = DateTime.Now;
                
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Thêm nhân viên thất bại!";
            return View("~/Views/Admin/QL_NhanVien/CreateOrEdit.cshtml", nhanVien);
        }

        // GET: Admin/NhanVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.NguoiDungs
                .FirstOrDefaultAsync(m => m.MaNguoiDung == id && m.VaiTro == "Staff");
                
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên nào phù hợp!";
                return RedirectToAction(nameof(Index));
            }          
            
            
            return View("~/Views/Admin/QL_NhanVien/CreateOrEdit.cshtml", nhanVien);
        }

        // POST: Admin/NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNguoiDung,TenDangNhap,MatKhau,Email,HoTen,VaiTro,TrangThai,NgayTao,LanDangNhapCuoi")] NguoiDung nhanVien)
        {
            if (id != nhanVien.MaNguoiDung)
            {
                return NotFound();
            }
            ModelState.Remove("MatKhau");

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the role is set to Staff
                    nhanVien.VaiTro = "Staff";
                    
                    // Get the existing user to check if password was changed
                    var existingNhanVien = await _context.NguoiDungs.AsNoTracking()
                        .FirstOrDefaultAsync(n => n.MaNguoiDung == id);
                        
                    if (existingNhanVien == null)
                    {
                        return NotFound();
                    }
                    
                    // If password field is empty, keep the old password
                    if (string.IsNullOrEmpty(nhanVien.MatKhau))
                    {
                        nhanVien.MatKhau = existingNhanVien.MatKhau;
                    }
                    
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Chỉnh sửa nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNguoiDung))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy nhân viên nào phù hợp!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View("~/Views/Admin/QL_NhanVien/CreateOrEdit.cshtml", nhanVien);
        }

        // GET: Admin/NhanVien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var nhanVien = await _context.NguoiDungs
                .FirstOrDefaultAsync(n => n.MaNguoiDung == id && n.VaiTro == "Staff");
                
            if (nhanVien != null)
            {
                _context.NguoiDungs.Remove(nhanVien);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa thất bại, không tìm thấy nhân viên nào phù hợp!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/NhanVien/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var nhanVien = await _context.NguoiDungs
                .FirstOrDefaultAsync(n => n.MaNguoiDung == id && n.VaiTro == "Staff");
                
            if (nhanVien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên nào phù hợp!";
                return RedirectToAction(nameof(Index));
            }
            
            // Toggle status
            nhanVien.TrangThai = nhanVien.TrangThai == "HoatDong" ? "KhongHoatDong" : "HoatDong";
            
            _context.Update(nhanVien);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Đã thay đổi trạng thái nhân viên thành {nhanVien.TrangThai}!";
            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(int id)
        {
            return _context.NguoiDungs.Any(e => e.MaNguoiDung == id && e.VaiTro == "Staff");
        }
    }
}
