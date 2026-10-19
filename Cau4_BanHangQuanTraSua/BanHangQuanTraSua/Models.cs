using Microsoft.EntityFrameworkCore;

namespace BanHangQuanTraSua;

public sealed class DanhMucMon { public int MaDM { get; set; } public string TenDM { get; set; } = ""; public List<MonUong> MonUongs { get; set; } = []; }
public sealed class MonUong { public int MaMon { get; set; } public string TenMon { get; set; } = ""; public decimal GiaBan { get; set; } public bool DangBan { get; set; } public int MaDM { get; set; } public DanhMucMon? DanhMuc { get; set; } public List<ChiTietHoaDon> ChiTiets { get; set; } = []; }
public sealed class HoaDon { public int MaHD { get; set; } public DateTime NgayLap { get; set; } public decimal TongTien { get; set; } public string HinhThucTT { get; set; } = ""; public bool DaThanhToan { get; set; } public List<ChiTietHoaDon> ChiTiets { get; set; } = []; }
public sealed class ChiTietHoaDon { public int MaHD { get; set; } public int MaMon { get; set; } public int SoLy { get; set; } public decimal DonGia { get; set; } public HoaDon? HoaDon { get; set; } public MonUong? MonUong { get; set; } }

public sealed class TraSuaContext(DbContextOptions<TraSuaContext> options) : DbContext(options)
{
    public DbSet<DanhMucMon> DanhMucMons => Set<DanhMucMon>();
    public DbSet<MonUong> MonUongs => Set<MonUong>();
    public DbSet<HoaDon> HoaDons => Set<HoaDon>();
    public DbSet<ChiTietHoaDon> ChiTietHoaDons => Set<ChiTietHoaDon>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<DanhMucMon>().ToTable("DanhMucMon").HasKey(x => x.MaDM);
        b.Entity<DanhMucMon>().Property(x => x.TenDM).HasMaxLength(50).IsRequired();
        b.Entity<MonUong>().ToTable("MonUong").HasKey(x => x.MaMon);
        b.Entity<MonUong>().Property(x => x.GiaBan).HasColumnType("decimal(18,0)");
        b.Entity<MonUong>().Property(x => x.TenMon).HasMaxLength(100).IsRequired();
        b.Entity<MonUong>().HasOne(x => x.DanhMuc).WithMany(x => x.MonUongs).HasForeignKey(x => x.MaDM);
        b.Entity<HoaDon>().ToTable("HoaDon").HasKey(x => x.MaHD);
        b.Entity<HoaDon>().Property(x => x.TongTien).HasColumnType("decimal(18,0)");
        b.Entity<HoaDon>().Property(x => x.HinhThucTT).HasMaxLength(20);
        b.Entity<ChiTietHoaDon>().ToTable("ChiTietHoaDon").HasKey(x => new { x.MaHD, x.MaMon });
        b.Entity<ChiTietHoaDon>().Property(x => x.DonGia).HasColumnType("decimal(18,0)");
        b.Entity<ChiTietHoaDon>().HasOne(x => x.HoaDon).WithMany(x => x.ChiTiets).HasForeignKey(x => x.MaHD);
        b.Entity<ChiTietHoaDon>().HasOne(x => x.MonUong).WithMany(x => x.ChiTiets).HasForeignKey(x => x.MaMon);
    }
}

public sealed class PaymentOptions { public string BankId { get; set; } = ""; public string AccountNo { get; set; } = ""; public string AccountName { get; set; } = ""; public string Template { get; set; } = "compact2"; public string ShopName { get; set; } = "Quán trà sữa"; }
public sealed class CartLine { public int MaMon { get; set; } public string TenMon { get; set; } = ""; public int SoLy { get; set; } public decimal DonGia { get; set; } public decimal ThanhTien => SoLy * DonGia; }
