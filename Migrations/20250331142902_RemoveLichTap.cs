using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GYM_Manage.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLichTap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToans_ThanhViens_ThanhVienMaThanhVien",
                table: "ThanhToans");

            migrationBuilder.DropTable(
                name: "LichTaps");

            migrationBuilder.DropIndex(
                name: "IX_ThanhToans_ThanhVienMaThanhVien",
                table: "ThanhToans");

            migrationBuilder.DropColumn(
                name: "ThanhVienMaThanhVien",
                table: "ThanhToans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThanhVienMaThanhVien",
                table: "ThanhToans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LichTaps",
                columns: table => new
                {
                    MaLichTap = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHuanLuyenVien = table.Column<int>(type: "int", nullable: false),
                    MaThanhVien = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichTaps", x => x.MaLichTap);
                    table.ForeignKey(
                        name: "FK_LichTaps_HuanLuyenViens_MaHuanLuyenVien",
                        column: x => x.MaHuanLuyenVien,
                        principalTable: "HuanLuyenViens",
                        principalColumn: "MaHuanLuyenVien",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichTaps_ThanhViens_MaThanhVien",
                        column: x => x.MaThanhVien,
                        principalTable: "ThanhViens",
                        principalColumn: "MaThanhVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToans_ThanhVienMaThanhVien",
                table: "ThanhToans",
                column: "ThanhVienMaThanhVien");

            migrationBuilder.CreateIndex(
                name: "IX_LichTaps_MaHuanLuyenVien",
                table: "LichTaps",
                column: "MaHuanLuyenVien");

            migrationBuilder.CreateIndex(
                name: "IX_LichTaps_MaThanhVien",
                table: "LichTaps",
                column: "MaThanhVien");

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToans_ThanhViens_ThanhVienMaThanhVien",
                table: "ThanhToans",
                column: "ThanhVienMaThanhVien",
                principalTable: "ThanhViens",
                principalColumn: "MaThanhVien");
        }
    }
}
