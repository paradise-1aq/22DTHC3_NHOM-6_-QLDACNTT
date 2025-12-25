using GYM_Manage.Data;
using GYM_Manage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net;
using System.Net.Mail;
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

namespace GYM_Manage.Controllers
{
    public class GoiTapController : Controller
    {
        private readonly GYM_DBcontext _context;

        public GoiTapController(GYM_DBcontext context)
        {
            _context = context;
        }

        // GET: GoiTap
        public async Task<IActionResult> Index()
        {
            var goiTaps = await _context.GoiTaps
                .Where(g => g.TrangThai == "Đang Hoạt Động" || g.TrangThai == "Khuyến Mãi")
                .ToListAsync();
            return View(goiTaps);
        }

        // GET: GoiTap/Details/5
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

            return View(goiTap);
        }

        // GET: GoiTap/DangKy/5
        public async Task<IActionResult> DangKy(int? id)
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

            // Kiểm tra người dùng đã đăng nhập chưa bằng Session
            string userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đăng ký gói tập.";
                return RedirectToAction("Login", "Auth");
            }

            return View(goiTap);
        }

        // POST: GoiTap/DangKy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(int id, [Bind("MaGoiTap")] GoiTap goiTap)
        {
            if (id != goiTap.MaGoiTap)
            {
                return NotFound();
            }

            // Lấy thông tin gói tập từ database
            var goiTapInfo = await _context.GoiTaps
                .FirstOrDefaultAsync(m => m.MaGoiTap == id);

            if (goiTapInfo == null)
            {
                return NotFound();
            }

            // Kiểm tra người dùng đã đăng nhập chưa bằng Session
            string userName = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(userName))
            {
                try
                {
                    // Lấy thông tin người dùng từ database
                    var nguoiDung = await _context.NguoiDungs
                        .FirstOrDefaultAsync(u => u.HoTen == userName && u.TrangThai == "HoatDong");
                    
                    if (nguoiDung == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                        return RedirectToAction("Login", "Auth");
                    }

                    // Chuyển hướng đến trang thanh toán
                    return RedirectToAction("ThanhToan", new { maGoiTap = id, maNguoiDung = nguoiDung.MaNguoiDung });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng ký gói tập: " + ex.Message);
                }
            }
            else
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để đăng ký gói tập.";
                return RedirectToAction("Login", "Auth");
            }

            return View(goiTapInfo);
        }

        // GET: GoiTap/ThanhToan
        public async Task<IActionResult> ThanhToan(int maGoiTap, int maNguoiDung)
        {
            var goiTap = await _context.GoiTaps
                .FirstOrDefaultAsync(g => g.MaGoiTap == maGoiTap);
                
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(n => n.MaNguoiDung == maNguoiDung);
                
            if (goiTap == null || nguoiDung == null)
            {
                return NotFound();
            }
            
            ViewBag.GoiTap = goiTap;
            ViewBag.NguoiDung = nguoiDung;
            
            return View();
        }
        
        // POST: GoiTap/ThanhToan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(int maGoiTap, int maNguoiDung, string phuongThucThanhToan, string ngaySinh, string gioiTinh, string soDienThoai, string diaChi, string ghiChu)
        {
            if (string.IsNullOrEmpty(phuongThucThanhToan))
            {
                ModelState.AddModelError("", "Vui lòng chọn phương thức thanh toán");
                return RedirectToAction("ThanhToan", new { maGoiTap, maNguoiDung });
            }
            
            var goiTap = await _context.GoiTaps
                .FirstOrDefaultAsync(g => g.MaGoiTap == maGoiTap);
                
            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(n => n.MaNguoiDung == maNguoiDung);
                
            if (goiTap == null || nguoiDung == null)
            {
                return NotFound();
            }
            
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tìm hoặc tạo ThanhVien mới
                    var thanhVien = await _context.ThanhViens
                        .FirstOrDefaultAsync(t => t.MaNguoiDung == maNguoiDung);
                        
                    if (thanhVien == null)
                    {
                        // Tạo thành viên mới
                        thanhVien = new ThanhVien
                        {
                            MaNguoiDung = maNguoiDung,
                            GioiTinh = gioiTinh,
                            SoDienThoai = soDienThoai,
                            DiaChi = diaChi,
                            NgaySinh = DateTime.TryParse(ngaySinh, out var parsedDate) ? parsedDate : null
                        };
                        
                        _context.ThanhViens.Add(thanhVien);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Cập nhật thông tin thành viên nếu đã tồn tại
                        thanhVien.GioiTinh = gioiTinh;
                        thanhVien.SoDienThoai = soDienThoai;
                        thanhVien.DiaChi = diaChi;
                        thanhVien.NgaySinh = DateTime.TryParse(ngaySinh, out var parsedDate) ? parsedDate : thanhVien.NgaySinh;
                        
                        _context.ThanhViens.Update(thanhVien);
                        await _context.SaveChangesAsync();
                    }
                    
                    // 2. Tạo bản ghi đăng ký gói tập
                    var ngayBatDau = DateTime.Now;
                    var ngayKetThuc = ngayBatDau.AddDays(goiTap.ThoiHan);
                    
                    var dangKyGoiTap = new DangKyGoiTap
                    {
                        MaThanhVien = thanhVien.MaThanhVien,
                        MaGoiTap = goiTap.MaGoiTap,
                        NgayBatDau = ngayBatDau,
                        NgayKetThuc = ngayKetThuc,
                        TrangThai = "HoatDong"
                    };
                    
                    _context.DangKyGoiTaps.Add(dangKyGoiTap);
                    await _context.SaveChangesAsync();
                    
                    // 3. Tạo bản ghi thanh toán
                    var thanhToan = new ThanhToan
                    {
                        MaThanhVien = thanhVien.MaThanhVien,
                        MaDangKyGoiTap = dangKyGoiTap.MaDangKy,
                        SoTien = goiTap.GiaTien,
                        NgayThanhToan = DateTime.Now,
                        PhuongThucThanhToan = phuongThucThanhToan,
                        TrangThai = "ThanhCong"
                    };
                    
                    _context.ThanhToans.Add(thanhToan);
                    await _context.SaveChangesAsync();
                    
                    // 4. Tạo hóa đơn
                    var hoaDon = new HoaDon
                    {
                        MaThanhVien = thanhVien.MaThanhVien,
                        MaThanhToan = thanhToan.MaThanhToan,
                        NgayHoaDon = DateTime.Now,
                        TongSoTien = goiTap.GiaTien,
                        TrangThai = "DaThanhToan"
                    };
                    
                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync();
                    
                    transaction.Commit();
                    
                    // 5. Tạo file PDF thẻ thành viên
                    string pdfThePath = TaoTheThanhVienPDF(thanhVien, dangKyGoiTap, goiTap, nguoiDung);
                    
                    // 6. Tạo file PDF hóa đơn
                    string pdfHoaDonPath = TaoHoaDonPDF(hoaDon, thanhToan, dangKyGoiTap, goiTap, nguoiDung, thanhVien);
                    
                    // 7. Gửi email với file PDF đính kèm
                    if (!string.IsNullOrEmpty(nguoiDung.Email))
                    {
                        await GuiEmailDangKyThanhCong(nguoiDung.Email, nguoiDung.HoTen, goiTap.TenGoiTap, dangKyGoiTap.NgayBatDau, dangKyGoiTap.NgayKetThuc, pdfThePath, pdfHoaDonPath);
                    }
                    
                    TempData["SuccessMessage"] = "Đăng ký gói tập và thanh toán thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình thanh toán: Hãy kiểm tra lại thông tin cá nhân " + ex.Message;
                    return RedirectToAction("ThanhToan", new { maGoiTap, maNguoiDung });
                }
            }
        }

        // Phương thức tạo file PDF thẻ thành viên
        private string TaoTheThanhVienPDF(ThanhVien thanhVien, DangKyGoiTap dangKyGoiTap, GoiTap goiTap, NguoiDung nguoiDung)
        {
            string fileName = $"TheThanhVien_{thanhVien.MaThanhVien}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs", fileName);

            // Đảm bảo thư mục tồn tại
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs"));

            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                // Tạo font chữ đậm
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Roboto_Condensed-Italic.ttf");
                PdfFont customFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Thiết lập màu sắc hiện đại
                Color primaryColor = new DeviceRgb(41, 128, 185); // Màu xanh dương hiện đại
                Color accentColor = new DeviceRgb(52, 152, 219); // Màu xanh nhạt hơn
                Color textColor = new DeviceRgb(44, 62, 80); // Màu đen xanh cho text
                Color lightBlueColor = new DeviceRgb(240, 248, 255); // Màu nền nhẹ
                Color lightPrimaryColor = new DeviceRgb(213, 230, 247); // Màu xanh nhạt (41, 128, 185) với 20% opacity
                Color veryLightPrimaryColor = new DeviceRgb(231, 242, 253); // Màu xanh nhạt (41, 128, 185) với 10% opacity
                Color lightGrayColor = new DeviceRgb(236, 240, 241); // Màu xám nhạt

                // Thêm logo gym từ URL
                try
                {
                    string logoUrl = "https://img.freepik.com/premium-vector/fitness-gym-logo-design_147826-117.jpg";
                    ImageData imageData = ImageDataFactory.Create(new Uri(logoUrl));
                    Image logo = new Image(imageData).ScaleToFit(150, 150).SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(logo);
                }
                catch (Exception)
                {
                    // Nếu không tải được ảnh, bỏ qua và tiếp tục
                    document.Add(new Paragraph("GYM FITNESS CENTER")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(18)
                        .SetFont(customFont)
                        .SetFontColor(primaryColor));
                }

                // Tiêu đề
                Paragraph header = new Paragraph("THẺ THÀNH VIÊN")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(24)
                    .SetFont(boldFont)
                    .SetFontColor(primaryColor);
                document.Add(header);

                // Thêm đường kẻ phân cách
                SolidLine line = new SolidLine(1f);
                line.SetColor(accentColor);
                LineSeparator ls = new LineSeparator(line);
                document.Add(ls);

                document.Add(new Paragraph("\n"));

                // Tạo bảng thông tin với viền tròn và màu nền
                Table table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
                table.SetBorder(new SolidBorder(accentColor, 1));
                table.SetBackgroundColor(lightBlueColor); // Màu nền nhẹ

                // Thêm ảnh đại diện thành viên (ảnh mặc định)
                try
                {
                    string avatarUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png";
                    ImageData avatarData = ImageDataFactory.Create(new Uri(avatarUrl));
                    Image avatar = new Image(avatarData).ScaleToFit(80, 80)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                    Cell avatarCell = new Cell(1, 2)
                        .Add(avatar)
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(10);
                    table.AddCell(avatarCell);
                }
                catch (Exception)
                {
                    // Bỏ qua nếu không tải được ảnh
                }

                // Thông tin thành viên với style hiện đại
                Cell labelCell, valueCell;

                labelCell = new Cell().Add(new Paragraph("Mã thành viên:").SetFont(customFont).SetFontColor(textColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                valueCell = new Cell().Add(new Paragraph(thanhVien.MaThanhVien.ToString()).SetFont(boldFont).SetFontColor(primaryColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                table.AddCell(labelCell);
                table.AddCell(valueCell);

                labelCell = new Cell().Add(new Paragraph("Họ và tên:").SetFont(customFont).SetFontColor(textColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                valueCell = new Cell().Add(new Paragraph(nguoiDung.HoTen).SetFont(boldFont).SetFontColor(primaryColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                table.AddCell(labelCell);
                table.AddCell(valueCell);

                labelCell = new Cell().Add(new Paragraph("Giới tính:").SetFont(customFont).SetFontColor(textColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                valueCell = new Cell().Add(new Paragraph(thanhVien.GioiTinh).SetFont(customFont).SetFontColor(primaryColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                table.AddCell(labelCell);
                table.AddCell(valueCell);

                labelCell = new Cell().Add(new Paragraph("Số điện thoại:").SetFont(customFont).SetFontColor(textColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                valueCell = new Cell().Add(new Paragraph(thanhVien.SoDienThoai).SetFont(customFont).SetFontColor(primaryColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                table.AddCell(labelCell);
                table.AddCell(valueCell);

                labelCell = new Cell().Add(new Paragraph("Email:").SetFont(customFont).SetFontColor(textColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                valueCell = new Cell().Add(new Paragraph(nguoiDung.Email).SetFont(customFont).SetFontColor(primaryColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                table.AddCell(labelCell);
                table.AddCell(valueCell);

                document.Add(table);

                document.Add(new Paragraph("\n"));

                // Thông tin gói tập trong bảng riêng với màu nền khác
                Table packageTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
                packageTable.SetBorder(new SolidBorder(primaryColor, 1));
                packageTable.SetBackgroundColor(veryLightPrimaryColor); // Màu nền nhẹ

                Cell packageHeader = new Cell(1, 2)
                    .Add(new Paragraph("THÔNG TIN GÓI TẬP").SetFont(boldFont).SetFontColor(primaryColor).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetBackgroundColor(lightPrimaryColor)
                    .SetPadding(5);
                packageTable.AddCell(packageHeader);

                labelCell = new Cell().Add(new Paragraph("Gói tập:").SetFont(customFont).SetFontColor(textColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                valueCell = new Cell().Add(new Paragraph(goiTap.TenGoiTap).SetFont(boldFont).SetFontColor(primaryColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                packageTable.AddCell(labelCell);
                packageTable.AddCell(valueCell);

                labelCell = new Cell().Add(new Paragraph("Ngày bắt đầu:").SetFont(customFont).SetFontColor(textColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                valueCell = new Cell().Add(new Paragraph(dangKyGoiTap.NgayBatDau.ToString("dd/MM/yyyy")).SetFont(customFont).SetFontColor(primaryColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                packageTable.AddCell(labelCell);
                packageTable.AddCell(valueCell);

                labelCell = new Cell().Add(new Paragraph("Ngày kết thúc:").SetFont(customFont).SetFontColor(textColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                valueCell = new Cell().Add(new Paragraph(dangKyGoiTap.NgayKetThuc.ToString("dd/MM/yyyy")).SetFont(customFont).SetFontColor(primaryColor)).SetBorder(Border.NO_BORDER).SetPadding(5);
                packageTable.AddCell(labelCell);
                packageTable.AddCell(valueCell);

                document.Add(packageTable);

                // Thông tin bổ sung với style hiện đại
                document.Add(new Paragraph("\n"));

                // Tạo bảng cho phần lưu ý
                Table noteTable = new Table(UnitValue.CreatePercentArray(1)).UseAllAvailableWidth();
                noteTable.SetBorder(new SolidBorder(accentColor, 1));
                noteTable.SetBackgroundColor(lightGrayColor);

                Cell noteHeader = new Cell()
                    .Add(new Paragraph("LƯU Ý QUAN TRỌNG").SetFont(boldFont).SetFontColor(primaryColor).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetBackgroundColor(lightGrayColor)
                    .SetPadding(5);
                noteTable.AddCell(noteHeader);

                Cell noteCell = new Cell()
                    .Add(new Paragraph("• Vui lòng mang theo thẻ này khi đến tập luyện\n• Thẻ không được chuyển nhượng cho người khác\n• Liên hệ: 0123456789 để được hỗ trợ").SetFont(customFont).SetFontColor(textColor))
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(10);
                noteTable.AddCell(noteCell);

                document.Add(noteTable);

                // Chữ ký và ngày tháng
                document.Add(new Paragraph("\n"));
                document.Add(new Paragraph($"Ngày cấp: {DateTime.Now:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFont(customFont)
                    .SetFontColor(textColor));

                // Thêm ảnh chữ ký
                try
                {
                    string signatureUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e4/Signature_of_Ann_Miller.svg/1280px-Signature_of_Ann_Miller.svg.png";
                    ImageData signatureData = ImageDataFactory.Create(new Uri(signatureUrl));
                    Image signature = new Image(signatureData)
                        .ScaleToFit(100, 50)
                        .SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                    document.Add(signature);
                }
                catch (Exception)
                {
                    // Bỏ qua nếu không tải được ảnh
                    document.Add(new Paragraph("Chữ ký quản lý")
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetFont(customFont)
                        .SetFontColor(textColor));
                }

                // Thêm mã QR
                try
                {
                    string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=100x100&data=MEMBER-{thanhVien.MaThanhVien}-{nguoiDung.HoTen}";
                    ImageData qrData = ImageDataFactory.Create(new Uri(qrUrl));
                    Image qrCode = new Image(qrData)
                        .ScaleToFit(80, 80)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    document.Add(qrCode);
                    document.Add(new Paragraph("Quét mã QR để xác thực thành viên")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(customFont)
                        .SetFontSize(8)
                        .SetFontColor(textColor));
                }
                catch (Exception)
                {
                    // Bỏ qua nếu không tải được mã QR
                }
            }

            return filePath;
        } // Phương thức tạo file PDF hóa đơn
        private string TaoHoaDonPDF(HoaDon hoaDon, ThanhToan thanhToan, DangKyGoiTap dangKyGoiTap, GoiTap goiTap, NguoiDung nguoiDung, ThanhVien thanhVien)
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
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Gói tập").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Thời hạn").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Đơn giá").SetFont(boldFont)));
                detailTable.AddHeaderCell(new Cell().Add(new Paragraph("Thành tiền").SetFont(boldFont)));


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

        // Phương thức gửi email
        private async Task GuiEmailDangKyThanhCong(string emailTo, string hoTen, string tenGoiTap, DateTime ngayBatDau, DateTime ngayKetThuc, string pdfThePath, string pdfHoaDonPath)
        {
            try
            {
                // Cấu hình email (nên đưa vào appsettings.json trong thực tế)
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string smtpUsername = "daohuy1692003@gmail.com"; // Thay bằng email thật
                string smtpPassword = "qzbf dknu hqbr ngxu"; // Thay bằng mật khẩu thật hoặc app password

                // Tạo message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtpUsername, "GYM Fitness Center");
                mail.To.Add(emailTo);
                mail.Subject = "Xác nhận đăng ký thành công - GYM Fitness Center";

                // Nội dung email
                mail.Body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Xác nhận đăng ký</title>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            line-height: 1.6;
            color: #333333;
            margin: 0;
            padding: 0;
            background-color: #f9f9f9;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            padding: 20px 0;
            border-bottom: 2px solid #f0f0f0;
        }}
        .header img {{
            max-width: 180px;
            height: auto;
            border-radius: 5px;
        }}
        .content {{
            padding: 30px 20px;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #888888;
            padding: 20px;
            border-top: 1px solid #f0f0f0;
        }}
        h1 {{
            color: #1a73e8;
            margin-top: 0;
            font-weight: 600;
            font-size: 24px;
        }}
        .highlight {{
            background-color: #f5f9ff;
            border-left: 4px solid #1a73e8;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .btn {{
            display: inline-block;
            background-color: #1a73e8;
            color: white;
            text-decoration: none;
            padding: 12px 24px;
            border-radius: 4px;
            font-weight: 500;
            margin: 20px 0;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        table th, table td {{
            padding: 12px;
            border-bottom: 1px solid #eeeeee;
            text-align: left;
        }}
        table th {{
            background-color: #f5f9ff;
            font-weight: 600;
        }}
        .social {{
            margin-top: 20px;
        }}
        .social a {{
            margin: 0 10px;
            text-decoration: none;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <!-- Logo từ Unsplash -->
            <img src='https://images.unsplash.com/photo-1534438327276-14e5300c3a48?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=300&q=80' alt='GYM Fitness Center Logo'>
            <h1>GYM FITNESS CENTER</h1>
        </div>
        
        <div class='content'>
            <h2>Xin chào {hoTen},</h2>
            
            <p>Chúc mừng bạn đã đăng ký thành công gói tập tại <strong>GYM Fitness Center</strong>!</p>
            
            <p>Chúng tôi rất vui mừng được đồng hành cùng bạn trong hành trình sức khỏe và thể hình của bạn. Dưới đây là chi tiết về gói tập bạn đã đăng ký:</p>
            
            <div class='highlight'>
                <table>
                    <tr>
                        <th>Thông tin</th>
                        <th>Chi tiết</th>
                    </tr>
                    <tr>
                        <td>Gói tập</td>
                        <td><strong>{tenGoiTap}</strong></td>
                    </tr>
                    <tr>
                        <td>Thời gian bắt đầu</td>
                        <td>{ngayBatDau:dd/MM/yyyy}</td>
                    </tr>
                    <tr>
                        <td>Thời gian kết thúc</td>
                        <td>{ngayKetThuc:dd/MM/yyyy}</td>
                    </tr>
                </table>
            </div>
            
            <p>Chúng tôi đã đính kèm thẻ thành viên và hóa đơn trong email này. Bạn có thể sử dụng bản điện tử hoặc in ra khi đến trung tâm.</p>
            
            <p>Một số lưu ý quan trọng:</p>
            <ul>
                <li>Vui lòng đến trước giờ tập ít nhất 15 phút để chuẩn bị</li>
                <li>Mang theo khăn và nước uống cá nhân</li>
                <li>Đặt lịch PT trước ít nhất 24 giờ qua ứng dụng hoặc tại quầy lễ tân</li>
            </ul>
            
            <p>Bạn có thể theo dõi lịch tập, đặt lịch với PT và nhiều tiện ích khác trên ứng dụng <strong>GYM Fitness Center</strong> (tải về từ App Store hoặc Google Play).</p>
            
            <center><a href='#' class='btn'>KHÁM PHÁ ỨNG DỤNG</a></center>
            
            <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi qua:</p>
            <ul>
                <li>Hotline: <strong>0123 456 789</strong> (8:00 - 22:00 hàng ngày)</li>
                <li>Email: <strong>support@gymfitnesscenter.com</strong></li>
                <li>Trực tiếp tại quầy lễ tân của trung tâm</li>
            </ul>
            
            <p>Chúng tôi rất mong được gặp bạn tại GYM Fitness Center!</p>
            
            <p>Trân trọng,<br>
            <strong>Ban Quản Lý<br>
            GYM Fitness Center</strong></p>
        </div>
        
        <div class='footer'>
            <div class='social'>
                <a href='#'>Facebook</a> | <a href='#'>Instagram</a> | <a href='#'>YouTube</a>
            </div>
            <p>Địa chỉ: 123 Đường Fitness, Quận Khỏe Mạnh, TP. Hồ Chí Minh</p>
            <p>&copy; 2025 GYM Fitness Center. Tất cả các quyền được bảo lưu.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>
        ";

                mail.IsBodyHtml = true;

                // Đính kèm file PDF
                if (System.IO.File.Exists(pdfThePath))
                {
                    mail.Attachments.Add(new Attachment(pdfThePath));
                }
                // Đính kèm file PDF
                if (System.IO.File.Exists(pdfHoaDonPath))
                {
                    mail.Attachments.Add(new Attachment(pdfHoaDonPath));
                }

                // Gửi email
                using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                {
                    smtp.EnableSsl = true;
                    smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không dừng quy trình
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            }
        }

        // GET: GoiTap/Search
        public async Task<IActionResult> Search(string keyword = null, int? soNgay = null)
        {
            var query = _context.GoiTaps
                .Where(g => (g.TrangThai == "Đang Hoạt Động" || g.TrangThai == "Khuyến Mãi"));
                
            // Tìm kiếm theo từ khóa nếu có
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(g => g.TenGoiTap.Contains(keyword) || g.MoTa.Contains(keyword));
            }
            
            // Tìm kiếm theo số ngày nếu có
            if (soNgay.HasValue && soNgay > 0)
            {
                query = query.Where(g => g.ThoiHan <= soNgay);
            }
            
            var goiTaps = await query.ToListAsync();
            
            if (goiTaps.Count == 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy gói tập phù hợp.";
                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = $"Tìm thấy {goiTaps.Count} gói tập phù hợp.";
            return View("Index", goiTaps);
        }
    }
}
