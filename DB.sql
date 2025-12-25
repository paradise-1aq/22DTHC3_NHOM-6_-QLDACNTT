-- Tạo CSDL Quản Lý Phòng Gym
CREATE DATABASE QuanLyPhongGym
GO

USE QuanLyPhongGym
GO

-- Bảng Người Dùng
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap NVARCHAR(50) UNIQUE NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    VaiTro NVARCHAR(20) NOT NULL CHECK (VaiTro IN (N'QuanTri', N'ThanhVien', N'NhanVien', N'HuanLuyenVien')),
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'HoatDong' CHECK (TrangThai IN (N'HoatDong', N'KhongHoatDong', N'TamKhoa')),
    NgayTao DATETIME DEFAULT GETDATE(),
    LanDangNhapCuoi DATETIME NULL
);

-- Bảng Thành Viên
CREATE TABLE ThanhVien (
    MaThanhVien INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT UNIQUE,
    NgaySinh DATE,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN (N'Nam', N'Nu', N'Khac')),
    SoDienThoai NVARCHAR(15),
    DiaChi NVARCHAR(MAX),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

-- Bảng Huấn Luyện Viên
CREATE TABLE HuanLuyenVien (
    MaHuanLuyenVien INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT UNIQUE,
    ChuyenMon NVARCHAR(100),
    ChungChi NVARCHAR(100),
    NgayTuyenDung DATE,
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

-- Bảng Gói Tập
CREATE TABLE GoiTap (
    MaGoiTap INT IDENTITY(1,1) PRIMARY KEY,
    TenGoiTap NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(MAX),
    ThoiHan INT NOT NULL,
    GiaTien DECIMAL(10,2) NOT NULL,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'HoatDong' CHECK (TrangThai IN (N'HoatDong', N'KhongHoatDong'))
);

-- Bảng Đăng Ký Gói Tập
CREATE TABLE DangKyGoiTap (
    MaDangKy INT IDENTITY(1,1) PRIMARY KEY,
    MaThanhVien INT,
    MaGoiTap INT,
    NgayBatDau DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'HoatDong' CHECK (TrangThai IN (N'HoatDong', N'HetHan', N'Huy')),
    FOREIGN KEY (MaThanhVien) REFERENCES ThanhVien(MaThanhVien),
    FOREIGN KEY (MaGoiTap) REFERENCES GoiTap(MaGoiTap)
);

-- Bảng Thiết Bị
CREATE TABLE ThietBi (
    MaThietBi INT IDENTITY(1,1) PRIMARY KEY,
    TenThietBi NVARCHAR(100) NOT NULL,
    DanhMuc NVARCHAR(50),
    NgayMua DATE,
    NgayBaoTriCuoi DATE,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'HoatDong' CHECK (TrangThai IN (N'HoatDong', N'CanSua', N'NgungSuDung'))
);

-- Bảng Lịch Tập
CREATE TABLE LichTap (
    MaLichTap INT IDENTITY(1,1) PRIMARY KEY,
    MaHuanLuyenVien INT,
    MaThanhVien INT,
    ThoiGianBatDau DATETIME NOT NULL,
    ThoiGianKetThuc DATETIME NOT NULL,
    MoTa NVARCHAR(MAX),
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'DaDatLich' CHECK (TrangThai IN (N'DaDatLich', N'HoanThanh', N'Huy')),
    FOREIGN KEY (MaHuanLuyenVien) REFERENCES HuanLuyenVien(MaHuanLuyenVien),
    FOREIGN KEY (MaThanhVien) REFERENCES ThanhVien(MaThanhVien)
);

-- Bảng Thanh Toán
CREATE TABLE ThanhToan (
    MaThanhToan INT IDENTITY(1,1) PRIMARY KEY,
    MaThanhVien INT,
    MaDangKyGoiTap INT,
    SoTien DECIMAL(10,2) NOT NULL,
    NgayThanhToan DATETIME DEFAULT GETDATE(),
    PhuongThucThanhToan NVARCHAR(20) NOT NULL CHECK (PhuongThucThanhToan IN (N'TheNganHang', N'TheTinDung', N'TienMat', N'ChuyenKhoan')),
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'ThanhCong' CHECK (TrangThai IN (N'ThanhCong', N'ChoXuLy', N'ThatBai')),
    FOREIGN KEY (MaThanhVien) REFERENCES ThanhVien(MaThanhVien),
    FOREIGN KEY (MaDangKyGoiTap) REFERENCES DangKyGoiTap(MaDangKy)
);

-- Bảng Hóa Đơn
CREATE TABLE HoaDon (
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    MaThanhVien INT,
    MaThanhToan INT,
    NgayHoaDon DATETIME DEFAULT GETDATE(),
    TongSoTien DECIMAL(10,2) NOT NULL,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'ChuaThanhToan' CHECK (TrangThai IN (N'DaThanhToan', N'ChuaThanhToan')),
    FOREIGN KEY (MaThanhVien) REFERENCES ThanhVien(MaThanhVien),
    FOREIGN KEY (MaThanhToan) REFERENCES ThanhToan(MaThanhToan)
);
GO

-- Thêm một số ràng buộc và index
CREATE INDEX IX_NguoiDung_TenDangNhap ON NguoiDung(TenDangNhap);
CREATE INDEX IX_ThanhVien_SoDienThoai ON ThanhVien(SoDienThoai);
CREATE INDEX IX_DangKyGoiTap_NgayBatDau ON DangKyGoiTap(NgayBatDau);
GO