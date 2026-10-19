using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using QuanLyNhaTro.Data;
using QuanLyNhaTro.Forms;
using QuanLyNhaTro.Services;

namespace QuanLyNhaTro;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Directory.CreateDirectory("Logs");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowUnhandled(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            ShowUnhandled(e.ExceptionObject as Exception ?? new Exception("Lỗi ngoài dự kiến."));

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .Build();
            var connection = config.GetConnectionString("NhaTro")
                ?? throw new InvalidOperationException("Thiếu ConnectionStrings:NhaTro trong appsettings.json.");
            var options = new DbContextOptionsBuilder<QLNhaTroContext>()
                .UseSqlServer(connection).Options;
            var http = new HttpClient { BaseAddress = new Uri("https://provinces.open-api.vn/"), Timeout = TimeSpan.FromSeconds(10) };
            Application.Run(new FrmKhachThue(options, new ProvinceApiClient(http)));
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Không khởi động được ứng dụng");
            MessageBox.Show("Không thể khởi động ứng dụng. Hãy kiểm tra cấu hình và file Logs.",
                "Lỗi khởi động", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { Log.CloseAndFlush(); }
    }

    private static void ShowUnhandled(Exception ex)
    {
        Log.Fatal(ex, "Lỗi chưa được xử lý toàn cục");
        if (!Application.MessageLoop) return;
        MessageBox.Show("Đã xảy ra lỗi, thông tin đã được ghi lại", "Thông báo",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
