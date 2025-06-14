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

        //Serilog
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
            services.AddSingleton<ReportGeneratorService>();
            services.AddHttpClient<WebhookService>();
        });
}

public class ReportWorker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName = "report_requests";
    private readonly ReportGeneratorService _reportGeneratorService;
    private readonly WebhookService _webhookService;
    private readonly ILogger<ReportWorker> _logger;

    public ReportWorker(ReportGeneratorService reportGeneratorService, WebhookService webhookService, ILogger<ReportWorker> logger)
    {
        _reportGeneratorService = reportGeneratorService;
        _webhookService = webhookService;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            ReportRequest? request = null;
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                //Deserialize
                request = JsonSerializer.Deserialize<ReportRequest>(message);

                if (request == null)
                    throw new InvalidOperationException($"Failed to deserialize message to ReportRequest. Message: {message}");

                _logger.LogInformation("Processing report: {ReportId}", request?.ReportId);

                //Create PDF
                var pdfBytes = _reportGeneratorService.GenerateReportPdf(request);

                //Webhook
                await _webhookService.SendWebhookAsync(
                    request.WebhookUrl,
                    request.ReportId,
                    pdfBytes,
                    success: true,
                    message: $"Report {request.ReportId} generated successfully"
                );

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing report: {ReportId}", request?.ReportId ?? "unknown");

                await _webhookService.SendWebhookAsync(
                    request?.WebhookUrl ?? string.Empty,
                    request?.ReportId ?? "unknown",
                    Array.Empty<byte>(),
                    success: false,
                    message: $"Error: {ex.Message}"
                );

                bool requeue = !(ex is InvalidOperationException && request == null);
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: requeue);
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping ReportWorker...");

        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}