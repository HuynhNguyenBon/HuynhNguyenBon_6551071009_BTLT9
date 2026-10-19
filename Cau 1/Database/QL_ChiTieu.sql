CREATE DATABASE QL_ChiTieu;
GO

USE QL_ChiTieu;
GO

CREATE TABLE ChiTieu
(
    MaCT INT IDENTITY(1,1) PRIMARY KEY,
    NgayChi DATE NOT NULL,
    NoiDung NVARCHAR(100) NOT NULL,
    LoaiChi NVARCHAR(30) NOT NULL,
    SoTien DECIMAL(18,0) NOT NULL,
    GhiChu NVARCHAR(200) NULL,

    CONSTRAINT CK_ChiTieu_SoTien CHECK (SoTien > 0)
);
GO

INSERT INTO ChiTieu
    (NgayChi, NoiDung, LoaiChi, SoTien, GhiChu)
VALUES
('2026-10-01', N'Ăn sáng', N'Ăn uống', 25000, N'Bánh mì'),
('2026-10-02', N'Đổ xăng', N'Đi lại', 50000, NULL),
('2026-10-03', N'Mua sách', N'Học tập', 120000, N'Sách lập trình'),
('2026-10-04', N'Đi xem phim', N'Giải trí', 90000, NULL),
('2026-10-05', N'Ăn trưa', N'Ăn uống', 35000, NULL),
('2026-10-06', N'Gửi xe', N'Đi lại', 10000, NULL),
('2026-10-07', N'Mua bút', N'Học tập', 20000, NULL),
('2026-10-08', N'Uống cà phê', N'Ăn uống', 30000, NULL),
('2026-10-09', N'Mua đồ dùng', N'Khác', 80000, NULL),
('2026-10-10', N'Đi chơi', N'Giải trí', 150000, NULL);
GO

SELECT * FROM ChiTieu;