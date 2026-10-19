using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BanHangQuanTraSua;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Directory.CreateDirectory("Logs");
        Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Report(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Report(e.ExceptionObject as Exception ?? new Exception("Lỗi hệ thống không xác định."));
        try
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", optional: false).Build();
            var cs = config.GetConnectionString("TraSua") ?? throw new InvalidOperationException("Thiếu ConnectionStrings:TraSua trong appsettings.json.");
            var pay = new PaymentOptions(); config.GetSection("Payment").Bind(pay);
            var dbOptions = new DbContextOptionsBuilder<TraSuaContext>()
                .UseSqlServer(cs, sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null))
                .Options;
            Application.Run(new FrmMain(dbOptions, pay, new HttpClient { Timeout = TimeSpan.FromSeconds(15) }));
        }
        catch (Exception ex) { Log.Fatal(ex, "Không khởi động được ứng dụng"); MessageBox.Show("Không mở được ứng dụng. Hãy chạy Database\\QL_TraSua.sql và kiểm tra appsettings.json. Chi tiết nằm trong thư mục Logs.", "Lỗi khởi động", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { Log.CloseAndFlush(); }
    }
    private static void Report(Exception ex) { Log.Error(ex, "Lỗi ứng dụng không xử lý được"); if (Application.MessageLoop) MessageBox.Show("Đã xảy ra lỗi. Thông tin đã được ghi vào Logs.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
}
