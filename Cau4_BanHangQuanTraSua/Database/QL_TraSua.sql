IF DB_ID(N'QL_TraSua') IS NULL CREATE DATABASE QL_TraSua;
GO
USE QL_TraSua;
GO
IF OBJECT_ID(N'dbo.DanhMucMon', N'U') IS NULL
 CREATE TABLE dbo.DanhMucMon(MaDM int IDENTITY(1,1) PRIMARY KEY, TenDM nvarchar(50) NOT NULL UNIQUE);
IF OBJECT_ID(N'dbo.MonUong', N'U') IS NULL
 CREATE TABLE dbo.MonUong(MaMon int IDENTITY(1,1) PRIMARY KEY, TenMon nvarchar(100) NOT NULL, GiaBan decimal(18,0) NOT NULL, DangBan bit NOT NULL CONSTRAINT DF_MonUong_DangBan DEFAULT(1), MaDM int NOT NULL REFERENCES dbo.DanhMucMon(MaDM));
IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
 CREATE TABLE dbo.HoaDon(MaHD int IDENTITY(1,1) PRIMARY KEY, NgayLap datetime NOT NULL, TongTien decimal(18,0) NOT NULL, HinhThucTT nvarchar(20) NOT NULL, DaThanhToan bit NOT NULL);
IF OBJECT_ID(N'dbo.ChiTietHoaDon', N'U') IS NULL
 CREATE TABLE dbo.ChiTietHoaDon(MaHD int NOT NULL REFERENCES dbo.HoaDon(MaHD), MaMon int NOT NULL REFERENCES dbo.MonUong(MaMon), SoLy int NOT NULL, DonGia decimal(18,0) NOT NULL, CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY(MaHD, MaMon));
IF NOT EXISTS(SELECT 1 FROM dbo.DanhMucMon)
 INSERT dbo.DanhMucMon(TenDM) VALUES (N'Trà sữa'),(N'Trà trái cây'),(N'Topping'),(N'Đồ ăn vặt');
IF NOT EXISTS(SELECT 1 FROM dbo.MonUong)
 INSERT dbo.MonUong(TenMon,GiaBan,DangBan,MaDM) VALUES
 (N'Trà sữa trân châu',35000,1,1),(N'Trà sữa matcha',40000,1,1),(N'Hồng trà sữa',32000,1,1),(N'Trà sữa khoai môn',38000,1,1),
 (N'Trà đào',35000,1,2),(N'Trà vải',35000,1,2),(N'Trà chanh dây',30000,1,2),
 (N'Trân châu đen',5000,1,3),(N'Trân châu trắng',6000,1,3),(N'Pudding trứng',7000,1,3),
 (N'Bánh flan',10000,1,4),(N'Khoai tây chiên',25000,1,4);
