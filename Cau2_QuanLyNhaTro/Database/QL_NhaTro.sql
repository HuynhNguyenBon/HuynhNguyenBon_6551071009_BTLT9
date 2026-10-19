IF DB_ID(N'QL_NhaTro') IS NULL CREATE DATABASE QL_NhaTro;
GO
USE QL_NhaTro;
GO
IF OBJECT_ID(N'dbo.PhongTro', N'U') IS NULL
BEGIN
CREATE TABLE dbo.PhongTro
(
    MaPhong int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhongTro PRIMARY KEY,
    TenPhong nvarchar(20) NOT NULL,
    GiaThue decimal(18,0) NOT NULL,
    TinhTrang nvarchar(20) NOT NULL CONSTRAINT DF_PhongTro_TinhTrang DEFAULT N'Trống'
);
END;
IF OBJECT_ID(N'dbo.KhachThue', N'U') IS NULL
BEGIN
CREATE TABLE dbo.KhachThue
(
    MaKhach int IDENTITY(1,1) NOT NULL CONSTRAINT PK_KhachThue PRIMARY KEY,
    HoTen nvarchar(100) NOT NULL,
    GioiTinh nvarchar(5) NOT NULL,
    NgaySinh date NOT NULL,
    SoDienThoai varchar(15) NOT NULL,
    MaTinh int NULL,
    TenTinh nvarchar(100) NOT NULL,
    MaPhuongXa int NULL,
    TenPhuongXa nvarchar(100) NOT NULL,
    DaDangKyTamTru bit NOT NULL CONSTRAINT DF_KhachThue_TamTru DEFAULT 0,
    MaPhong int NOT NULL,
    CONSTRAINT FK_KhachThue_PhongTro FOREIGN KEY (MaPhong) REFERENCES dbo.PhongTro(MaPhong)
);
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.PhongTro)
BEGIN
    INSERT dbo.PhongTro(TenPhong,GiaThue,TinhTrang) VALUES
    (N'P01',1500000,N'Trống'),(N'P02',1600000,N'Trống'),(N'P03',1800000,N'Trống'),
    (N'P04',2000000,N'Trống'),(N'P05',1700000,N'Trống'),(N'P06',2200000,N'Trống');
END;
IF NOT EXISTS (SELECT 1 FROM dbo.KhachThue)
BEGIN
    INSERT dbo.KhachThue(HoTen,GioiTinh,NgaySinh,SoDienThoai,MaTinh,TenTinh,MaPhuongXa,TenPhuongXa,DaDangKyTamTru,MaPhong) VALUES
    (N'Nguyễn Văn An',N'Nam','2004-05-12','0912345678',1,N'Thành phố Hà Nội',1,N'Phường Ba Đình',1,3),
    (N'Trần Thị Bình',N'Nữ','2002-09-18','0876543210',1,N'Thành phố Hà Nội',2,N'Phường Ngọc Hà',0,1),
    (N'Lê Văn Cường',N'Nam','2001-03-21','0901234567',38,N'Tỉnh Thanh Hóa',NULL,N'Phường Đông Sơn',0,2),
    (N'Phạm Minh Đức',N'Nam','1999-11-02','0987654321',1,N'Thành phố Hà Nội',3,N'Phường Giảng Võ',1,4);
END;
UPDATE p SET TinhTrang=CASE WHEN EXISTS (SELECT 1 FROM dbo.KhachThue k WHERE k.MaPhong=p.MaPhong) THEN N'Đang thuê' ELSE N'Trống' END
FROM dbo.PhongTro p;
GO
