using Microsoft.EntityFrameworkCore;
using QuanLyNhaTro.Models;

namespace QuanLyNhaTro.Data;

public partial class QLNhaTroContext(DbContextOptions<QLNhaTroContext> options) : DbContext(options)
{
    public virtual DbSet<PhongTro> PhongTros => Set<PhongTro>();
    public virtual DbSet<KhachThue> KhachThues => Set<KhachThue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhongTro>(entity =>
        {
            entity.HasKey(e => e.MaPhong);
            entity.ToTable("PhongTro");
            entity.Property(e => e.TenPhong).HasMaxLength(20);
            entity.Property(e => e.GiaThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TinhTrang).HasMaxLength(20);
        });
        modelBuilder.Entity<KhachThue>(entity =>
        {
            entity.HasKey(e => e.MaKhach);
            entity.ToTable("KhachThue");
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.GioiTinh).HasMaxLength(5);
            entity.Property(e => e.NgaySinh).HasColumnType("date");
            entity.Property(e => e.SoDienThoai).HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.TenTinh).HasMaxLength(100);
            entity.Property(e => e.TenPhuongXa).HasMaxLength(100);
            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.KhachThues)
                .HasForeignKey(d => d.MaPhong).OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_KhachThue_PhongTro");
        });
    }
}
