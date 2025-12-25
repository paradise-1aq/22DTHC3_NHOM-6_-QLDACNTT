using GYM_Manage.Data;
using GYM_Manage.Models;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace GYM_Manage.Controllers.Admin
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly GYM_DBcontext _context;

        public DashboardController(GYM_DBcontext context)
        {
            _context = context;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            var viewModel = new DashboardViewModel
            {
                TongThanhVien = await _context.ThanhViens.CountAsync(),
                ThanhVienMoi = await _context.ThanhViens.Where(tv => tv.NguoiDung.NgayTao >= DateTime.Now.AddDays(-30)).CountAsync(),
                TongNguoiDung = await _context.NguoiDungs.CountAsync(),
                TongThietBi = await _context.ThietBis.CountAsync(),
                ThietBiHoatDong = await _context.ThietBis.Where(tb => tb.TrangThai == "Hoạt động tốt").CountAsync(),
                ThietBiBaoTri = await _context.ThietBis.Where(tb => tb.TrangThai == "Cần bảo trì").CountAsync(),
                TongThanhToan = await _context.ThanhToans.CountAsync(),
                ThanhToanThanhCong = await _context.ThanhToans.Where(tt => tt.TrangThai == "ThanhCong").CountAsync(),
                DoanhThuThang = await _context.ThanhToans
                    .Where(tt => tt.NgayThanhToan.Month == DateTime.Now.Month && tt.NgayThanhToan.Year == DateTime.Now.Year && tt.TrangThai == "ThanhCong")
                    .SumAsync(tt => tt.SoTien),
                DoanhThuNam = await _context.ThanhToans
                    .Where(tt => tt.NgayThanhToan.Year == DateTime.Now.Year && tt.TrangThai == "ThanhCong")
                    .SumAsync(tt => tt.SoTien)
            };

            // Thống kê doanh thu theo tháng trong năm hiện tại
            viewModel.DoanhThuTheoThang = await _context.ThanhToans
                .Where(tt => tt.NgayThanhToan.Year == DateTime.Now.Year && tt.TrangThai == "ThanhCong")
                .GroupBy(tt => tt.NgayThanhToan.Month)
                .Select(g => new DoanhThuTheoThang
                {
                    Thang = g.Key,
                    DoanhThu = g.Sum(tt => tt.SoTien)
                })
                .OrderBy(dt => dt.Thang)
                .ToListAsync();

            // Thống kê thanh toán gần đây
            viewModel.ThanhToanGanDay = await _context.ThanhToans
                .Include(tt => tt.ThanhVien)
                .ThenInclude(tv => tv.NguoiDung)
                .Include(tt => tt.DangKyGoiTap)
                .ThenInclude(dk => dk.GoiTap)
                .OrderByDescending(tt => tt.NgayThanhToan)
                .Take(10)
                .ToListAsync();

            return View("~/Views/Admin/Dashboard/Index.cshtml", viewModel);
        }

        // GET: Admin/Dashboard/ThongKeThanhVien
        public async Task<IActionResult> ThongKeThanhVien()
        {
            var thanhViens = await _context.ThanhViens
                .Include(tv => tv.NguoiDung)
                .OrderByDescending(tv => tv.NguoiDung.NgayTao)
                .ToListAsync();

            return View("~/Views/Admin/Dashboard/ThongKeThanhVien.cshtml", thanhViens);
        }

        // GET: Admin/Dashboard/ThongKeThanhToan
        public async Task<IActionResult> ThongKeThanhToan(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var query = _context.ThanhToans
                .Include(tt => tt.ThanhVien)
                .ThenInclude(tv => tv.NguoiDung)
                .Include(tt => tt.DangKyGoiTap)
                .ThenInclude(dk => dk.GoiTap)
                .AsQueryable();

            if (tuNgay.HasValue)
            {
                query = query.Where(tt => tt.NgayThanhToan >= tuNgay.Value);
            }

            if (denNgay.HasValue)
            {
                query = query.Where(tt => tt.NgayThanhToan <= denNgay.Value.AddDays(1));
            }

            var thanhToans = await query.OrderByDescending(tt => tt.NgayThanhToan).ToListAsync();

            ViewBag.TuNgay = tuNgay;
            ViewBag.DenNgay = denNgay;
            ViewBag.TongDoanhThu = thanhToans.Where(tt => tt.TrangThai == "ThanhCong").Sum(tt => tt.SoTien);
            ViewBag.TongThanhToan = thanhToans.Count;
            ViewBag.ThanhToanThanhCong = thanhToans.Count(tt => tt.TrangThai == "ThanhCong");

            return View("~/Views/Admin/Dashboard/ThongKeThanhToan.cshtml", thanhToans);
        }

        // GET: Admin/Dashboard/ThongKeThietBi
        public async Task<IActionResult> ThongKeThietBi()
        {
            var thietBis = await _context.ThietBis.OrderBy(tb => tb.DanhMuc).ToListAsync();

            ViewBag.TongThietBi = thietBis.Count;
            ViewBag.ThietBiHoatDong = thietBis.Count(tb => tb.TrangThai == "HoatDong");
            ViewBag.ThietBiBaoTri = thietBis.Count(tb => tb.TrangThai == "BaoTri");
            ViewBag.ThietBiHong = thietBis.Count(tb => tb.TrangThai == "Hong");

            return View("~/Views/Admin/Dashboard/ThongKeThietBi.cshtml", thietBis);
        }

        // GET: Admin/Dashboard/ThongKeNguoiDung
        public async Task<IActionResult> ThongKeNguoiDung()
        {
            var nguoiDungs = await _context.NguoiDungs.OrderByDescending(nd => nd.NgayTao).ToListAsync();

            ViewBag.TongNguoiDung = nguoiDungs.Count;
            ViewBag.NguoiDungHoatDong = nguoiDungs.Count(nd => nd.TrangThai == "HoatDong");
            ViewBag.NguoiDungAdmin = nguoiDungs.Count(nd => nd.VaiTro == "Admin");
            ViewBag.NguoiDungKhach = nguoiDungs.Count(nd => nd.VaiTro == "Khach");

            return View("~/Views/Admin/Dashboard/ThongKeNguoiDung.cshtml", nguoiDungs);
        }

        // GET: Admin/Dashboard/XuatBaoCaoDoanhThu
        public async Task<IActionResult> XuatBaoCaoDoanhThu(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            var query = _context.ThanhToans
                .Include(tt => tt.ThanhVien)
                .ThenInclude(tv => tv.NguoiDung)
                .Include(tt => tt.DangKyGoiTap)
                .ThenInclude(dk => dk.GoiTap)
                .Where(tt => tt.TrangThai == "ThanhCong")
                .AsQueryable();

            if (tuNgay.HasValue)
            {
                query = query.Where(tt => tt.NgayThanhToan >= tuNgay.Value);
            }

            if (denNgay.HasValue)
            {
                query = query.Where(tt => tt.NgayThanhToan <= denNgay.Value.AddDays(1));
            }

            var thanhToans = await query.OrderByDescending(tt => tt.NgayThanhToan).ToListAsync();

            string filePath = TaoBaoCaoDoanhThuPDF(thanhToans, tuNgay, denNgay);
            string fileName = Path.GetFileName(filePath);

            return PhysicalFile(filePath, "application/pdf", fileName);
        }

        private string TaoBaoCaoDoanhThuPDF(List<ThanhToan> thanhToans, DateTime? tuNgay, DateTime? denNgay)
        {
            string fileName = $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs", fileName);

            // Đảm bảo thư mục tồn tại
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs"));

            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf, PageSize.A4))
            {
                // Tạo font chữ
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Tiêu đề báo cáo
                document.Add(new Paragraph("BÁO CÁO DOANH THU")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetFont(boldFont));

                document.Add(new Paragraph("GYM FITNESS CENTER")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetFont(boldFont));

                // Thời gian báo cáo
                string thoiGianBaoCao = "Thời gian: ";
                if (tuNgay.HasValue && denNgay.HasValue)
                {
                    thoiGianBaoCao += $"Từ {tuNgay.Value:dd/MM/yyyy} đến {denNgay.Value:dd/MM/yyyy}";
                }
                else if (tuNgay.HasValue)
                {
                    thoiGianBaoCao += $"Từ {tuNgay.Value:dd/MM/yyyy} đến nay";
                }
                else if (denNgay.HasValue)
                {
                    thoiGianBaoCao += $"Đến {denNgay.Value:dd/MM/yyyy}";
                }
                else
                {
                    thoiGianBaoCao += "Tất cả thời gian";
                }

                document.Add(new Paragraph(thoiGianBaoCao)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetFont(regularFont));

                document.Add(new Paragraph($"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(10)
                    .SetFont(regularFont));

                document.Add(new Paragraph("\n"));

                // Thông tin tổng quan
                Table infoTable = new Table(2).UseAllAvailableWidth();
                infoTable.AddCell(new Cell().Add(new Paragraph("Tổng số giao dịch:").SetFont(boldFont)).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph($"{thanhToans.Count}").SetFont(regularFont)).SetBorder(Border.NO_BORDER));

                infoTable.AddCell(new Cell().Add(new Paragraph("Tổng doanh thu:").SetFont(boldFont)).SetBorder(Border.NO_BORDER));
                infoTable.AddCell(new Cell().Add(new Paragraph($"{thanhToans.Sum(tt => tt.SoTien):N0} VNĐ").SetFont(regularFont)).SetBorder(Border.NO_BORDER));

                document.Add(infoTable);
                document.Add(new Paragraph("\n"));

                // Thống kê theo phương thức thanh toán
                var thongKeTheoPhThuc = thanhToans
                    .GroupBy(tt => tt.PhuongThucThanhToan)
                    .Select(g => new
                    {
                        PhuongThuc = g.Key,
                        SoLuong = g.Count(),
                        TongTien = g.Sum(tt => tt.SoTien)
                    })
                    .OrderByDescending(x => x.TongTien)
                    .ToList();

                document.Add(new Paragraph("THỐNG KÊ THEO PHƯƠNG THỨC THANH TOÁN")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(14)
                    .SetFont(boldFont));

                Table ptttTable = new Table(3).UseAllAvailableWidth();
                ptttTable.AddHeaderCell(new Cell().Add(new Paragraph("Phương thức").SetFont(boldFont)));
                ptttTable.AddHeaderCell(new Cell().Add(new Paragraph("Số lượng").SetFont(boldFont)));
                ptttTable.AddHeaderCell(new Cell().Add(new Paragraph("Tổng tiền").SetFont(boldFont)));

                foreach (var item in thongKeTheoPhThuc)
                {
                    ptttTable.AddCell(new Cell().Add(new Paragraph(item.PhuongThuc).SetFont(regularFont)));
                    ptttTable.AddCell(new Cell().Add(new Paragraph(item.SoLuong.ToString()).SetFont(regularFont)));
                    ptttTable.AddCell(new Cell().Add(new Paragraph($"{item.TongTien:N0} VNĐ").SetFont(regularFont)));
                }

                document.Add(ptttTable);
                document.Add(new Paragraph("\n"));

                // Thống kê theo gói tập
                var thongKeTheoGoiTap = thanhToans
                    .GroupBy(tt => tt.DangKyGoiTap.GoiTap.TenGoiTap)
                    .Select(g => new
                    {
                        TenGoiTap = g.Key,
                        SoLuong = g.Count(),
                        TongTien = g.Sum(tt => tt.SoTien)
                    })
                    .OrderByDescending(x => x.TongTien)
                    .ToList();

                document.Add(new Paragraph("THỐNG KÊ THEO GÓI TẬP")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(14)
                    .SetFont(boldFont));

                Table goiTapTable = new Table(3).UseAllAvailableWidth();
                goiTapTable.AddHeaderCell(new Cell().Add(new Paragraph("Tên gói tập").SetFont(boldFont)));
                goiTapTable.AddHeaderCell(new Cell().Add(new Paragraph("Số lượng").SetFont(boldFont)));
                goiTapTable.AddHeaderCell(new Cell().Add(new Paragraph("Tổng tiền").SetFont(boldFont)));

                foreach (var item in thongKeTheoGoiTap)
                {
                    goiTapTable.AddCell(new Cell().Add(new Paragraph(item.TenGoiTap).SetFont(regularFont)));
                    goiTapTable.AddCell(new Cell().Add(new Paragraph(item.SoLuong.ToString()).SetFont(regularFont)));
                    goiTapTable.AddCell(new Cell().Add(new Paragraph($"{item.TongTien:N0} VNĐ").SetFont(regularFont)));
                }

                document.Add(goiTapTable);
                document.Add(new Paragraph("\n"));

                // Chi tiết giao dịch
                document.Add(new Paragraph("CHI TIẾT GIAO DỊCH")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(14)
                    .SetFont(boldFont));

                Table detailTable = new Table(5).UseAllAvailableWidth();
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Mã GD").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Thời gian").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Khách hàng").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Gói tập").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Số tiền").SetFont(boldFont)));

                foreach (var tt in thanhToans)
                {
                    detailTable.AddCell(new Cell().Add(new Paragraph(tt.MaThanhToan.ToString()).SetFont(regularFont)));
                    detailTable.AddCell(new Cell().Add(new Paragraph(tt.NgayThanhToan.ToString("dd/MM/yyyy HH:mm")).SetFont(regularFont)));
                    detailTable.AddCell(new Cell().Add(new Paragraph(tt.ThanhVien.NguoiDung.HoTen).SetFont(regularFont)));
                    detailTable.AddCell(new Cell().Add(new Paragraph(tt.DangKyGoiTap.GoiTap.TenGoiTap).SetFont(regularFont)));
                    detailTable.AddCell(new Cell().Add(new Paragraph($"{tt.SoTien:N0} VNĐ").SetFont(regularFont)));
                }

                document.Add(detailTable);

                // Chân trang
                document.Add(new Paragraph("\n\n"));
                document.Add(new Paragraph("Báo cáo được tạo tự động từ hệ thống GYM Fitness Center")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetFont(regularFont));
            }

            return filePath;
        }
    }

    public class DashboardViewModel
    {
        // Thống kê tổng quan
        public int TongThanhVien { get; set; }
        public int ThanhVienMoi { get; set; }
        public int TongNguoiDung { get; set; }
        public int TongThietBi { get; set; }
        public int ThietBiHoatDong { get; set; }
        public int ThietBiBaoTri { get; set; }
        public int TongThanhToan { get; set; }
        public int ThanhToanThanhCong { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal DoanhThuNam { get; set; }

        // Thống kê doanh thu theo tháng
        public List<DoanhThuTheoThang> DoanhThuTheoThang { get; set; }

        // Thanh toán gần đây
        public List<ThanhToan> ThanhToanGanDay { get; set; }
    }

    public class DoanhThuTheoThang
    {
        public int Thang { get; set; }
        public decimal DoanhThu { get; set; }
    }
}
