using GYM_Manage.Data;
using GYM_Manage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GYM_Manage.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMomoService _momoService;
        private readonly GYM_DBcontext _context; // dùng đúng DbContext đã cấu hình DI

        public PaymentController(IMomoService momoService, GYM_DBcontext context)
        {
            _momoService = momoService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(int maGoiTap)
        {
            if (maGoiTap <= 0)
            {
                TempData["ErrorMessage"] = "Thiếu hoặc sai mã gói tập.";
                return RedirectToAction("Index", "GoiTap");
            }

            // Kiểm tra gói tập tồn tại (đồng thời để lấy route id khi Redirect lỗi)
            var goiTap = await _context.GoiTaps
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.MaGoiTap == maGoiTap);

            if (goiTap == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy gói tập.";
                return RedirectToAction("Index", "GoiTap");
            }

            // Gọi đúng chữ ký interface (chỉ truyền maGoiTap)
            var response = await _momoService.CreatePaymentAsync(maGoiTap);

            if (response != null && !string.IsNullOrEmpty(response.PayUrl))
            {
                return Redirect(response.PayUrl);
            }

            TempData["ErrorMessage"] = "Không thể tạo liên kết thanh toán với MoMo.";
            return RedirectToAction("ThanhToan", "GoiTap", new { id = goiTap.MaGoiTap });
        }

        // MoMo redirect (return URL)
        public IActionResult MomoPaymentResult(string orderId, string requestId, string resultCode, string message, int maGoiTap)
        {
            if (resultCode == "0")
            {
                TempData["SuccessMessage"] = "Thanh toán MoMo thành công!";
                return RedirectToAction("XacNhanThanhToan", "GoiTap", new { id = maGoiTap });
            }

            TempData["ErrorMessage"] = "Thanh toán MoMo thất bại. Vui lòng thử lại.";
            return RedirectToAction("ThanhToan", "GoiTap", new { id = maGoiTap });
        }
    }
}
