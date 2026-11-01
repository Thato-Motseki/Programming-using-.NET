using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SEIMS.Data;
using SEIMS.Services;
using System.Windows.Forms;

namespace SEIMS.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            using (IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<SEIMSDbContext>(options =>
                        options.UseSqlServer(
                            context.Configuration.GetConnectionString(
                                "SEIMSConnection")));

                    services.AddTransient<MainForm>();

                    // MainForm requires these services in its constructor.
                    services.AddTransient<StockMonitor>();
                    services.AddSingleton<FileLogger>(_ =>
                        new FileLogger(
                            Path.Combine(
                                AppContext.BaseDirectory,
                                "logs",
                                "seims.log")));

                    services.AddTransient<InventoryReports>();
                    services.AddTransient<InventoryDataService>();
                    services.AddTransient<InventoryXmlExporter>();
                })
                .Build())
            {
                using (IServiceScope scope = host.Services.CreateScope())
                {
                    MainForm mainForm =
                        scope.ServiceProvider.GetRequiredService<MainForm>();

                    Application.Run(mainForm);
                }
            }
        }
    }
}