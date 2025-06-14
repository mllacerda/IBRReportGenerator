using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportGenerator.Domain.Models;
using ReportGenerator.Worker.Services;
using System.Text;
using System.Text.Json;

namespace ReportGenerator.Worker;

public class Program
{
    public static async Task Main(string[] args)
    {
        // QuestPDF Community License
        QuestPDF.Settings.License = LicenseType.Community;

        await CreateHostBuilder(args).Build().RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<ReportWorker>();
                services.AddSingleton<ReportGeneratorService>();
            });
}

public class ReportWorker : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName = "report_requests";
    private readonly ReportGeneratorService _reportGeneratorService;

    public ReportWorker(ReportGeneratorService reportGeneratorService)
    {
        _reportGeneratorService = reportGeneratorService;

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<ReportRequest>(message);

                Console.WriteLine($"Processing report: {request?.ReportId}");

                // Create PDF
                var pdfBytes = _reportGeneratorService.GenerateReportPdf(request);

                //Test downloading PDF
                //File.WriteAllBytes($"report_{request.ReportId}.pdf", pdfBytes);

                // TODO: Enviar o webhook com o PDF
                Console.WriteLine($"PDF generated for report: {request?.ReportId}");

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing report: {ex.Message}");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}