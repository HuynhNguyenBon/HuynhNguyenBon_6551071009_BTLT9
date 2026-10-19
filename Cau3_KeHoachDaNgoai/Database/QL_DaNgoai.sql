IF DB_ID(N'QL_DaNgoai') IS NULL CREATE DATABASE QL_DaNgoai;
GO
USE QL_DaNgoai;
GO
IF OBJECT_ID(N'dbo.ChuyenDi', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChuyenDi
    (
        MaChuyen int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChuyenDi PRIMARY KEY,
        TenChuyen nvarchar(100) NOT NULL,
        DiaDiem nvarchar(100) NOT NULL,
        NgayDi date NOT NULL,
        SoNguoi int NOT NULL,
        NganSach decimal(18,0) NOT NULL,
        PhuongTien nvarchar(30) NOT NULL,
        DaHoanThanh bit NOT NULL CONSTRAINT DF_ChuyenDi_HoanThanh DEFAULT 0,
        GhiChu nvarchar(200) NULL
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.ChuyenDi)
BEGIN
    INSERT dbo.ChuyenDi(TenChuyen,DiaDiem,NgayDi,SoNguoi,NganSach,PhuongTien,DaHoanThanh,GhiChu) VALUES
    (N'Cắm trại đồi chè',N'Đà Lạt',DATEADD(day,6,CONVERT(date,GETDATE())),6,3500000,N'Ô tô',0,NULL),
    (N'Cắm trại đồi chè',N'Bia Hạ',DATEADD(day,6,CONVERT(date,GETDATE())),6,3500000,N'Ô tô',0,NULL),
    (N'Cắm trại đồi chè',N'Đà Lạt',DATEADD(day,6,CONVERT(date,GETDATE())),6,3500000,N'Ô tô',0,NULL),
    (N'Cắm trại ban tối',N'Địa Hạng',DATEADD(day,9,CONVERT(date,GETDATE())),4,2800000,N'Xe khách',0,NULL),
    (N'Cắm trại ven hồ',N'Bia Hạ',DATEADD(day,11,CONVERT(date,GETDATE())),6,3500000,N'Ô tô',0,NULL),
    (N'Cắm trại thác đá chảy rìa',N'Phú Ninh',DATEADD(day,14,CONVERT(date,GETDATE())),8,4200000,N'Xe máy',0,NULL),
    (N'Dã ngoại cuối tuần',N'Sa Pa',DATEADD(day,4,CONVERT(date,GETDATE())),5,5000000,N'Tàu hỏa',0,NULL),
    (N'Đi biển mùa thu',N'Vũng Tàu',DATEADD(day,2,CONVERT(date,GETDATE())),3,2500000,N'Ô tô',0,NULL);
END;
GO
