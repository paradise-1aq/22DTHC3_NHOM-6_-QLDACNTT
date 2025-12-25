using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GYM_Manage.Controllers
{
    public class AboutController : Controller
    {
        private readonly GYM_DBcontext _context;

        public AboutController(GYM_DBcontext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AboutViewModel
            {
                HuanLuyenViens = await _context.HuanLuyenViens.Include(h => h.NguoiDung).ToListAsync(),
                ThietBis = await _context.ThietBis.ToListAsync()
            };

            return View(viewModel);
        }
    }

    public class AboutViewModel
    {
        public List<HuanLuyenVien> HuanLuyenViens { get; set; }
        public List<ThietBi> ThietBis { get; set; }
    }
}
