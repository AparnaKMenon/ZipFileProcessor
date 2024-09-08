using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ZipFileProcessor.Services;

namespace ZipFileProcessor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .CreateLogger();

            var host = CreateHostBuilder(args).Build();
          
            // Ensure the app is disposed on exit
            using (host)
            {
                try
                {
                    Log.Information("Starting application");
                    var zipProcessingService = host.Services.GetRequiredService<IZipProcessingService>();
                    zipProcessingService.ProcessZipFilesAsync().Wait();

                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Application failed");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                   
                    // Load secrets.json from Current Directory
                    var secretFilePath = Path.Combine(Directory.GetCurrentDirectory(), "secrets.json");

                    if (File.Exists(secretFilePath))
                    {
                        config.AddJsonFile(secretFilePath, optional: true, reloadOnChange: true);
                    }
                })

                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
                })
                .ConfigureServices((context, services) =>
                {
                    // Configure settings
                    services.Configure<EmailSettings>(context.Configuration.GetSection("EmailSettings"));
                    services.Configure<ZipProcessingSettings>(context.Configuration.GetSection("ZipProcessingSettings"));

                    // Register services
                    services.AddSingleton<IEmailService, EmailService>();
                    services.AddSingleton<ILoggerService, LoggerService>();
                    services.AddSingleton<IZipProcessingService, ZipProcessingService>();

                });
    }
}
