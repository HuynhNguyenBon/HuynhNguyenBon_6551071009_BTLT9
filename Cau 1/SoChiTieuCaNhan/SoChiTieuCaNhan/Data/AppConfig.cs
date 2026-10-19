using Microsoft.EntityFrameworkCore;
using SoChiTieuCaNhan.Models;

namespace SoChiTieuCaNhan.Data
{
    public static class AppConfig
    {
        public static QL_ChiTieuContext TaoContext()
        {
            var options =
                new DbContextOptionsBuilder<QL_ChiTieuContext>()
                .UseSqlServer(
                    @"Server=(localdb)\MSSQLLocalDB;Database=QL_ChiTieu;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

            return new QL_ChiTieuContext(options);
        }
    }
}