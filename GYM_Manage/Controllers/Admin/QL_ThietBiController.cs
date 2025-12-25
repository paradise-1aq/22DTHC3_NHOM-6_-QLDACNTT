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
    public class QL_ThietBiController : Controller
    {
        private readonly GYM_DBcontext _context;

        public QL_ThietBiController(GYM_DBcontext context)
        {
            _context = context;
        }

        // GET: Admin/ThietBi
        public async Task<IActionResult> Index()
        {
            var thietBis = await _context.ThietBis.ToListAsync();
            return View("~/Views/Admin/QL_ThietBi/Index.cshtml", thietBis);
        }
        // GET: Admin/ThietBi/Search
        public async Task<IActionResult> Search(string keyword)
        {
            var thietBis = await _context.ThietBis.Where(t => t.TenThietBi.Contains(keyword)).ToListAsync();
            if (thietBis.Count == 0)
            {
                TempData["ErrorMessage"] = "Chúng tôi không tìm thấy thiết bị nào phù hợp!";
                
                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = $"Chúng tôi tìm thấy {thietBis.Count} thiết bị phù hợp!";
            return View("~/Views/Admin/QL_ThietBi/Index.cshtml", thietBis);
        }

        // GET: Admin/ThietBi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thietBi = await _context.ThietBis
                .FirstOrDefaultAsync(m => m.MaThietBi == id);
            if (thietBi == null)
            {
                return NotFound();
            }

            return View(thietBi);
        }

        // GET: Admin/ThietBi/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/QL_ThietBi/CreateOrEdit.cshtml", new ThietBi());
        }

        // POST: Admin/ThietBi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenThietBi,DanhMuc,NgayMua,NgayBaoTriCuoi,TrangThai")] ThietBi thietBi)
        {
            if (ModelState.IsValid)
            {
                _context.Add(thietBi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm thành công!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Thêm thất bại!";
            return View("~/Views/Admin/QL_ThietBi/CreateOrEdit.cshtml");
        }

        // GET: Admin/ThietBi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thietBi = await _context.ThietBis.FindAsync(id);
            if (thietBi == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thiết bị nào phù hợp!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/QL_ThietBi/CreateOrEdit.cshtml", thietBi);
        }

        // POST: Admin/ThietBi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaThietBi,TenThietBi,DanhMuc,NgayMua,NgayBaoTriCuoi,TrangThai")] ThietBi thietBi)
        {
            if (id != thietBi.MaThietBi)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thietBi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Chỉnh sửa thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThietBiExists(thietBi.MaThietBi))
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thiết bị nào phù hợp!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View("~/Views/Admin/QL_ThietBi/CreateOrEdit.cshtml", thietBi);
        }

        // GET: Admin/ThietBi/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {

            var thietBi = await _context.ThietBis.FindAsync(id);
            if (thietBi != null)
            {
                _context.ThietBis.Remove(thietBi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa thất bại không tìm thấy thiết bị nào phù hợp!";
            }

            return RedirectToAction(nameof(Index));
        }



        private bool ThietBiExists(int id)
        {
            return _context.ThietBis.Any(e => e.MaThietBi == id);
        }
    }
}
