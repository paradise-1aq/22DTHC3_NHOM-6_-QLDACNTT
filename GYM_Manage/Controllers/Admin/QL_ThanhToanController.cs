using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
using iText.IO.Font;
using static iText.Kernel.Font.PdfFontFactory;
using iText.Kernel.Pdf.Canvas.Draw;

namespace GYM_Manage.Controllers.Admin
{
    [Area("Admin")]
    public class QL_ThanhToanController : Controller
    {
        private readonly GYM_DBcontext _context;

        public QL_ThanhToanController(GYM_DBcontext context)
        {
            _context = context;
        }

        // GET: Admin/QL_ThanhToan
        public async Task<IActionResult> Index()
        {
            var thanhToans = await _context.ThanhToans
                .Include(t => t.ThanhVien)
                    .ThenInclude(tv => tv.NguoiDung)
                .Include(t => t.DangKyGoiTap)
                    .ThenInclude(d => d.GoiTap)
                .OrderByDescending(t => t.NgayThanhToan)
                .ToListAsync();

            return View("~/Views/Admin/QL_ThanhToan/Index.cshtml", thanhToans);
        }

        // GET: Admin/QL_ThanhToan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thanhToan = await _context.ThanhToans
                .Include(t => t.ThanhVien)
                    .ThenInclude(tv => tv.NguoiDung)
                .Include(t => t.DangKyGoiTap)
                    .ThenInclude(d => d.GoiTap)
                .FirstOrDefaultAsync(t => t.MaThanhToan == id);

            if (thanhToan == null)
            {
                return NotFound();
            }

            // Lấy hóa đơn liên quan
            var hoaDon = await _context.HoaDons
                .FirstOrDefaultAsync(h => h.MaThanhToan == thanhToan.MaThanhToan);

            ViewBag.HoaDon = hoaDon;

            return View("~/Views/Admin/QL_ThanhToan/Details.cshtml", thanhToan);
        }

       
        // GET: Admin/QL_ThanhToan/TaoHoaDonPDF/5
        public async Task<IActionResult> TaoHoaDonPDF(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thanhToan = await _context.ThanhToans
                .Include(t => t.ThanhVien)
                    .ThenInclude(tv => tv.NguoiDung)
                .Include(t => t.DangKyGoiTap)
                    .ThenInclude(d => d.GoiTap)
                .FirstOrDefaultAsync(t => t.MaThanhToan == id);

            var hoaDon = await _context.HoaDons
                .FirstOrDefaultAsync(h => h.MaThanhToan == id);

            if (hoaDon != null)
            {
                hoaDon.ThanhToan = thanhToan;
                hoaDon.ThanhVien = thanhToan?.ThanhVien;
            }

            if (hoaDon == null)
            {
                return NotFound();
            }

            string pdfPath = TaoHoaDonPDF(hoaDon);
            
            // Trả về file PDF
            byte[] fileBytes = System.IO.File.ReadAllBytes(pdfPath);
            return File(fileBytes, "application/pdf", $"HoaDon_{hoaDon.MaHoaDon}.pdf");
        }

        private string TaoHoaDonPDF(HoaDon hoaDon)
        {
            string fileName = $"HoaDon_{hoaDon.MaHoaDon}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs", fileName);

            // Đảm bảo thư mục tồn tại
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs"));

            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                // Tạo font chữ
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Roboto_Condensed-Italic.ttf");
                PdfFont customFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, EmbeddingStrategy.FORCE_EMBEDDED);

                // Tiêu đề
                Paragraph header = new Paragraph("HÓA ĐƠN THANH TOÁN")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetFont(customFont);
                document.Add(header);

                document.Add(new Paragraph("GYM FITNESS CENTER")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetFont(customFont));

                document.Add(new Paragraph("\n"));

                // Lấy thông tin người dùng và thành viên
                var thanhToan = hoaDon.ThanhToan;
                var thanhVien = hoaDon.ThanhVien;
                var nguoiDung = thanhVien.NguoiDung;
                var dangKyGoiTap = thanhToan.DangKyGoiTap;
                var goiTap = dangKyGoiTap.GoiTap;

                // Thông tin hóa đơn
                Table infoTable = new Table(2).UseAllAvailableWidth();

                infoTable.AddCell(new Cell().Add(new Paragraph("Mã hóa đơn:").SetFont(customFont)).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(hoaDon.MaHoaDon.ToString()).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                infoTable.AddCell(new Cell().Add(new Paragraph("Ngày tạo:").SetFont(customFont)).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(hoaDon.NgayHoaDon.ToString("dd/MM/yyyy HH:mm:ss")).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                infoTable.AddCell(new Cell().Add(new Paragraph("Khách hàng:").SetFont(customFont)).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(nguoiDung.HoTen).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                infoTable.AddCell(new Cell().Add(new Paragraph("Số điện thoại:").SetFont(customFont)).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(thanhVien.SoDienThoai).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                infoTable.AddCell(new Cell().Add(new Paragraph("Email:").SetFont(customFont)).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph(nguoiDung.Email).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                document.Add(infoTable);

                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("CHI TIẾT THANH TOÁN")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14)
                    .SetFont(customFont));
                document.Add(new Paragraph("\n"));

                // Chi tiết thanh toán
                Table detailTable = new Table(4).UseAllAvailableWidth();

                // Header
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Goi tap").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Thoi han").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Đon gia").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Thanh tien").SetFont(boldFont)));


                // Data
                detailTable.AddCell(new Cell().Add(new Paragraph(goiTap.TenGoiTap).SetFont(customFont)));
                detailTable.AddCell(new Cell().Add(new Paragraph($"{goiTap.ThoiHan} ngày").SetFont(customFont)));
                detailTable.AddCell(new Cell().Add(new Paragraph($"{goiTap.GiaTien:N0} VNĐ").SetFont(customFont)));
                detailTable.AddCell(new Cell().Add(new Paragraph($"{goiTap.GiaTien:N0} VNĐ").SetFont(customFont)));

                document.Add(detailTable);

                document.Add(new Paragraph("\n"));

                // Tổng tiền
                Table totalTable = new Table(2).UseAllAvailableWidth();
                totalTable.AddCell(new Cell().Add(new Paragraph("Tổng tiền thanh toán:").SetFont(customFont).SimulateBold()).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
                totalTable.AddCell(new Cell().Add(new Paragraph($"{hoaDon.TongSoTien:N0} VNĐ").SetFont(boldFont)).SetBorder(Border.NO_BORDER));

                totalTable.AddCell(new Cell().Add(new Paragraph("Phương thức thanh toán:").SetFont(customFont)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
                totalTable.AddCell(new Cell().Add(new Paragraph(thanhToan.PhuongThucThanhToan).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                totalTable.AddCell(new Cell().Add(new Paragraph("Trạng thái:").SetFont(customFont)).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.RIGHT));
                totalTable.AddCell(new Cell().Add(new Paragraph(hoaDon.TrangThai).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                document.Add(totalTable);

                // Thông tin thời hạn
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph("THÔNG TIN GÓI TẬP")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14)
                    .SetFont(customFont));
                document.Add(new Paragraph("\n"));

                Table periodTable = new Table(2).UseAllAvailableWidth();
                periodTable.AddCell(new Cell().Add(new Paragraph("Ngày bắt đầu:").SetFont(customFont)).SetBorder(Border.NO_BORDER));
                periodTable.AddCell(new Cell().Add(new Paragraph(dangKyGoiTap.NgayBatDau.ToString("dd/MM/yyyy")).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                periodTable.AddCell(new Cell().Add(new Paragraph("Ngày kết thúc:").SetFont(customFont)).SetBorder(Border.NO_BORDER));
                periodTable.AddCell(new Cell().Add(new Paragraph(dangKyGoiTap.NgayKetThuc.ToString("dd/MM/yyyy")).SetFont(customFont)).SetBorder(Border.NO_BORDER));

                document.Add(periodTable);

               

                // Chân trang
                document.Add(new Paragraph("\n\n"));
                document.Add(new Paragraph("Cảm ơn quý khách đã sử dụng dịch vụ của chúng tôi!")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(customFont));
                document.Add(new Paragraph("Hotline: 0123456789 - Email: info@gymfitness.com")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(customFont)
                    .SetFontSize(10));
            }

            return filePath;
        }

        // GET: Admin/QL_ThanhToan/Search
        public async Task<IActionResult> Search(string keyword, DateTime? tuNgay, DateTime? denNgay, string trangThai)
        {
            var query = _context.ThanhToans
                .Include(t => t.ThanhVien)
                    .ThenInclude(tv => tv.NguoiDung)
                .Include(t => t.DangKyGoiTap)
                    .ThenInclude(d => d.GoiTap)
                .AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => 
                    t.ThanhVien.NguoiDung.HoTen.Contains(keyword) || 
                    t.ThanhVien.SoDienThoai.Contains(keyword) ||
                    t.DangKyGoiTap.GoiTap.TenGoiTap.Contains(keyword));
            }

            // Tìm kiếm theo khoảng thời gian
            if (tuNgay.HasValue)
            {
                query = query.Where(t => t.NgayThanhToan >= tuNgay.Value);
            }

            if (denNgay.HasValue)
            {
                // Đảm bảo lấy đến hết ngày kết thúc
                var endDate = denNgay.Value.AddDays(1).AddSeconds(-1);
                query = query.Where(t => t.NgayThanhToan <= endDate);
            }

            // Tìm kiếm theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(t => t.TrangThai == trangThai);
            }

            var thanhToans = await query.OrderByDescending(t => t.NgayThanhToan).ToListAsync();

            return View("~/Views/Admin/QL_ThanhToan/Index.cshtml", thanhToans);
        }

    
    }
}
