using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GYM_Manage.Controllers.Admin
{
    [Area("Admin")]
    public class QL_ThanhVienController : Controller
    {
        private readonly GYM_DBcontext _context;

        public QL_ThanhVienController(GYM_DBcontext context)
        {
            _context = context;
        }

        // GET: Admin/QL_ThanhVien
        public async Task<IActionResult> Index()
        {
            var thanhViens = await _context.ThanhViens
                .Include(t => t.NguoiDung)
                .OrderByDescending(t => t.MaThanhVien)
                .ToListAsync();

            return View("~/Views/Admin/QL_ThanhVien/Index.cshtml", thanhViens);
        }

      

        // POST: Admin/QL_ThanhVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thanhVien = await _context.ThanhViens
                .Include(t => t.NguoiDung)
                .FirstOrDefaultAsync(t => t.MaThanhVien == id);

            if (thanhVien == null)
            {
                return NotFound();
            }

            // Kiểm tra xem thành viên có thanh toán nào không
            var coThanhToan = await _context.ThanhToans
                .AnyAsync(t => t.MaThanhVien == id);

            if (coThanhToan)
            {
                TempData["ErrorMessage"] = "Không thể xóa thành viên này vì đã có lịch sử thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            // Xóa người dùng liên quan
            if (thanhVien.NguoiDung != null)
            {
                _context.NguoiDungs.Remove(thanhVien.NguoiDung);
            }

            _context.ThanhViens.Remove(thanhVien);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa thành viên thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/QL_ThanhVien/Search
        public async Task<IActionResult> Search(string keyword, string gioiTinh)
        {
            var query = _context.ThanhViens
                .Include(t => t.NguoiDung)
                .AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => 
                    t.NguoiDung.HoTen.Contains(keyword) || 
                    t.NguoiDung.Email.Contains(keyword) ||
                    t.SoDienThoai.Contains(keyword) ||
                    t.DiaChi.Contains(keyword));
            }

            // Tìm kiếm theo giới tính
            if (!string.IsNullOrEmpty(gioiTinh))
            {
                query = query.Where(t => t.GioiTinh == gioiTinh);
            }

            var thanhViens = await query.OrderByDescending(t => t.MaThanhVien).ToListAsync();

            return View("~/Views/Admin/QL_ThanhVien/Index.cshtml", thanhViens);
        }
    }
}
