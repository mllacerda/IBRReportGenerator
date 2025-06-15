using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportGenerator.Domain.Models;
using ReportGenerator.Worker.Services;
using Serilog;
using System.Text;
using System.Text.Json;

namespace ReportGenerator.Worker;

public class Program
{
    public static async Task Main(string[] args)
    {
        //QuestPDF License
        QuestPDF.Settings.License = LicenseType.Community;

        //Logger
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        await CreateHostBuilder(args).Build().RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<ReportWorker>();
            services.AddSingleton<IReportGeneratorService, ReportGeneratorService>();
            services.AddSingleton<IWebhookService, WebhookService>();
            services.AddHttpClient<IWebhookService, WebhookService>();
            services.Configure<RabbitMQSettings>(hostContext.Configuration.GetSection("RabbitMQ"));
        });
}