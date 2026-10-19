using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using KeHoachDaNgoai.Data;
using KeHoachDaNgoai.Forms;

namespace KeHoachDaNgoai;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Directory.CreateDirectory("Logs");
        Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ReportUnhandled(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            ReportUnhandled(e.ExceptionObject as Exception ?? new Exception("Unexpected error."));
        try
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
            var cs = config.GetConnectionString("DaNgoai") ?? throw new InvalidOperationException("Missing connection string DaNgoai.");
            var options = new DbContextOptionsBuilder<QLDaNgoaiContext>().UseSqlServer(cs).Options;
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            Application.Run(new FrmKeHoach(options, http));
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            MessageBox.Show("Không thể khởi động ứng dụng. Hãy kiểm tra cấu hình và thư mục Logs.", "Lỗi khởi động", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { Log.CloseAndFlush(); }
    }

    private static void ReportUnhandled(Exception ex)
    {
        Log.Fatal(ex, "Unhandled application exception");
        if (Application.MessageLoop)
            MessageBox.Show("Đã xảy ra lỗi, thông tin đã được ghi lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
