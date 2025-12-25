using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GYM_Manage.Controllers
{
    public class BaiVietController : Controller
    {
        private readonly GYM_DBcontext _context;
        private readonly ILogger<BaiVietController> _logger;

        public BaiVietController(GYM_DBcontext context, ILogger<BaiVietController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: BaiViet
        public async Task<IActionResult> Index()
        {
            var baiViet = await _context.BaiViets
                .Include(b => b.NguoiTao)
                .Where(b => b.TrangThai == "HienThi")
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            return View(baiViet);
        }

        // GET: BaiViet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiViet = await _context.BaiViets
                .Include(b => b.NguoiTao)
                .FirstOrDefaultAsync(m => m.MaBaiViet == id && m.TrangThai == "HienThi");

            if (baiViet == null)
            {
                return NotFound();
            }

            // Lấy các bài viết liên quan (cùng người tạo hoặc mới nhất)
            var relatedPosts = await _context.BaiViets
                .Where(b => b.MaBaiViet != id && b.TrangThai == "HienThi")
                .OrderByDescending(b => b.NgayDang)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedPosts = relatedPosts;

            // Tăng lượt xem nếu cần
            // baiViet.LuotXem += 1;
            // await _context.SaveChangesAsync();

            return View(baiViet);
        }

        // GET: BaiViet/Search
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction(nameof(Index));
            }

            var searchResults = await _context.BaiViets
                .Include(b => b.NguoiTao)
                .Where(b => b.TrangThai == "HienThi" && 
                           (b.TieuDe.Contains(keyword) || 
                            b.MoTaNgan.Contains(keyword) || 
                            b.NoiDung.Contains(keyword)))
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.ResultCount = searchResults.Count;

            return View(searchResults);
        }
    }
}
