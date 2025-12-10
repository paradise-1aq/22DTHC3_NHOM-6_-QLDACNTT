using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GYM_Manage.Migrations
{
    /// <inheritdoc />
    public partial class data1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoiTaps",
                columns: table => new
                {
                    MaGoiTap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenGoiTap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhDemo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiHan = table.Column<int>(type: "int", nullable: false),
                    GiaTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoiTaps", x => x.MaGoiTap);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LanDangNhapCuoi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.MaNguoiDung);
                });

            migrationBuilder.CreateTable(
                name: "ThietBis",
                columns: table => new
                {
                    MaThietBi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenThietBi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DanhMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayMua = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayBaoTriCuoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietBis", x => x.MaThietBi);
                });

            migrationBuilder.CreateTable(
                name: "BaiViets",
                columns: table => new
                {
                    MaBaiViet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTaNgan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(555)", maxLength: 555, nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDNguoiTao = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiViets", x => x.MaBaiViet);
                    table.ForeignKey(
                        name: "FK_BaiViets_NguoiDungs_IDNguoiTao",
                        column: x => x.IDNguoiTao,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HuanLuyenViens",
                columns: table => new
                {
                    MaHuanLuyenVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChuyenMon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChungChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTuyenDung = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HuanLuyenViens", x => x.MaHuanLuyenVien);
                    table.ForeignKey(
                        name: "FK_HuanLuyenViens_NguoiDungs_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "ThanhViens",
                columns: table => new
                {
                    MaThanhVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhViens", x => x.MaThanhVien);
                    table.ForeignKey(
                        name: "FK_ThanhViens_NguoiDungs_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDungs",
                        principalColumn: "MaNguoiDung");
                });

            migrationBuilder.CreateTable(
                name: "DangKyGoiTaps",
                columns: table => new
                {
                    MaDangKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThanhVien = table.Column<int>(type: "int", nullable: false),
                    MaGoiTap = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyGoiTaps", x => x.MaDangKy);
                    table.ForeignKey(
                        name: "FK_DangKyGoiTaps_GoiTaps_MaGoiTap",
                        column: x => x.MaGoiTap,
                        principalTable: "GoiTaps",
                        principalColumn: "MaGoiTap",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DangKyGoiTaps_ThanhViens_MaThanhVien",
                        column: x => x.MaThanhVien,
                        principalTable: "ThanhViens",
                        principalColumn: "MaThanhVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToans",
                columns: table => new
                {
                    MaThanhToan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThanhVien = table.Column<int>(type: "int", nullable: false),
                    MaDangKyGoiTap = table.Column<int>(type: "int", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToans", x => x.MaThanhToan);
                    table.ForeignKey(
                        name: "FK_ThanhToans_DangKyGoiTaps_MaDangKyGoiTap",
                        column: x => x.MaDangKyGoiTap,
                        principalTable: "DangKyGoiTaps",
                        principalColumn: "MaDangKy");
                    table.ForeignKey(
                        name: "FK_ThanhToans_ThanhViens_MaThanhVien",
                        column: x => x.MaThanhVien,
                        principalTable: "ThanhViens",
                        principalColumn: "MaThanhVien");
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThanhVien = table.Column<int>(type: "int", nullable: false),
                    MaThanhToan = table.Column<int>(type: "int", nullable: false),
                    NgayHoaDon = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongSoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDons_ThanhToans_MaThanhToan",
                        column: x => x.MaThanhToan,
                        principalTable: "ThanhToans",
                        principalColumn: "MaThanhToan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaDons_ThanhViens_MaThanhVien",
                        column: x => x.MaThanhVien,
                        principalTable: "ThanhViens",
                        principalColumn: "MaThanhVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "NguoiDungs",
                columns: new[] { "MaNguoiDung", "Email", "HoTen", "LanDangNhapCuoi", "MatKhau", "NgayTao", "TenDangNhap", "TrangThai", "VaiTro" },
                values: new object[,]
                {
                    { 10, "admin@gmail.com", "Quản trị viên", null, "admin", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin", "Hoạt động", "Admin" },
                    { 11, "user@gmail.com", "Member so 1", null, "user", new DateTime(2025, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "user", "Hoạt động", "Member" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiViets_IDNguoiTao",
                table: "BaiViets",
                column: "IDNguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyGoiTaps_MaGoiTap",
                table: "DangKyGoiTaps",
                column: "MaGoiTap");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyGoiTaps_MaThanhVien",
                table: "DangKyGoiTaps",
                column: "MaThanhVien");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_MaThanhToan",
                table: "HoaDons",
                column: "MaThanhToan");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_MaThanhVien",
                table: "HoaDons",
                column: "MaThanhVien");

            migrationBuilder.CreateIndex(
                name: "IX_HuanLuyenViens_MaNguoiDung",
                table: "HuanLuyenViens",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_TenDangNhap",
                table: "NguoiDungs",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_MaDangKyGoiTap",
                table: "ThanhToans",
                column: "MaDangKyGoiTap");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_MaThanhVien",
                table: "ThanhToans",
                column: "MaThanhVien");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhViens_MaNguoiDung",
                table: "ThanhViens",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhViens_SoDienThoai",
                table: "ThanhViens",
                column: "SoDienThoai",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiViets");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "HuanLuyenViens");

            migrationBuilder.DropTable(
                name: "ThietBis");

            migrationBuilder.DropTable(
                name: "ThanhToans");

            migrationBuilder.DropTable(
                name: "DangKyGoiTaps");

            migrationBuilder.DropTable(
                name: "GoiTaps");

            migrationBuilder.DropTable(
                name: "ThanhViens");

            migrationBuilder.DropTable(
                name: "NguoiDungs");
        }
    }
}
