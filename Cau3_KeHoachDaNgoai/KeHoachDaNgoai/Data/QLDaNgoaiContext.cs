using Microsoft.EntityFrameworkCore;
using KeHoachDaNgoai.Models;

namespace KeHoachDaNgoai.Data;

public partial class QLDaNgoaiContext(DbContextOptions<QLDaNgoaiContext> options) : DbContext(options)
{
    public virtual DbSet<ChuyenDi> ChuyenDis => Set<ChuyenDi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChuyenDi>(entity =>
        {
            entity.HasKey(e => e.MaChuyen);
            entity.ToTable("ChuyenDi");
            entity.Property(e => e.TenChuyen).HasMaxLength(100);
            entity.Property(e => e.DiaDiem).HasMaxLength(100);
            entity.Property(e => e.NgayDi).HasColumnType("date");
            entity.Property(e => e.NganSach).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.PhuongTien).HasMaxLength(30);
            entity.Property(e => e.GhiChu).HasMaxLength(200);
        });
    }
}
