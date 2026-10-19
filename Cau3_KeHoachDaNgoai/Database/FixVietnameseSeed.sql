USE QL_DaNgoai;
GO
-- Restore the original sample records only if their text has mojibake markers.
IF EXISTS (SELECT 1 FROM dbo.ChuyenDi WHERE MaChuyen BETWEEN 1 AND 8
    AND (TenChuyen LIKE N'%Ã%' OR TenChuyen LIKE N'%áº%' OR TenChuyen LIKE N'%Ä%' OR TenChuyen LIKE N'%á»%' OR TenChuyen LIKE N'%Å%'))
BEGIN
    UPDATE dbo.ChuyenDi SET TenChuyen=N'Cắm trại đồi chè', DiaDiem=N'Đà Lạt', PhuongTien=N'Ô tô' WHERE MaChuyen=1;
    UPDATE dbo.ChuyenDi SET TenChuyen=N'Cắm trại đồi chè', DiaDiem=N'Bia Hạ', PhuongTien=N'Ô tô' WHERE MaChuyen=2;
    UPDATE dbo.ChuyenDi SET TenChuyen=N'Cắm trại đồi chè', DiaDiem=N'Đà Lạt', PhuongTien=N'Ô tô' WHERE MaChuyen=3;
    UPDATE dbo.ChuyenDi SET TenChuyen=N'Cắm trại ban tối', DiaDiem=N'Địa Hạng', PhuongTien=N'Xe khách' WHERE MaChuyen=4;
    UPDATE dbo.ChuyenDi SET TenChuyen=N'Cắm trại ven hồ', DiaDiem=N'Bia Hạ', PhuongTien=N'Ô tô' WHERE MaChuyen=5;
    UPDATE dbo.ChuyenDi SET TenChuyen=N'Cắm trại thác đá chảy rìa', DiaDiem=N'Phú Ninh', PhuongTien=N'Xe máy' WHERE MaChuyen=6;
    UPDATE dbo.ChuyenDi SET TenChuyen=N'Dã ngoại cuối tuần', DiaDiem=N'Sa Pa', PhuongTien=N'Tàu hỏa' WHERE MaChuyen=7;
    UPDATE dbo.ChuyenDi SET TenChuyen=N'Đi biển mùa thu', DiaDiem=N'Vũng Tàu', PhuongTien=N'Ô tô' WHERE MaChuyen=8;
END;
GO
