using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportGenerator.Domain.Models;
using ReportGenerator.Worker.Services;
using System.Text;
using System.Text.Json;

namespace ReportGenerator.Worker
{
    public class ReportWorker : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly IReportGeneratorService _reportGeneratorService;
        private readonly IWebhookService _webhookService;
        private readonly ILogger<ReportWorker> _logger;

        public ReportWorker(IReportGeneratorService reportGeneratorService, 
            IWebhookService webhookService, ILogger<ReportWorker> logger, IOptions<RabbitMQSettings> settings)
        {
            _logger = logger;
            _reportGeneratorService = reportGeneratorService;
            _webhookService = webhookService;

            //Settings
            var rabbitMQSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _queueName = rabbitMQSettings.QueueName;

            var factory = new ConnectionFactory
            {
                HostName = rabbitMQSettings.HostName,
                UserName = rabbitMQSettings.UserName,
                Password = rabbitMQSettings.Password
            };

            //Connection
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
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
                    _logger.LogInformation("Processing report: {ReportId}", ea.BasicProperties.CorrelationId);

                    //Deserialize
                    request = JsonSerializer.Deserialize<ReportRequest>(message);

                    if (request == null)
                        throw new InvalidOperationException($"Failed to deserialize message to ReportRequest. Message: {message}");

                    _logger.LogInformation("Processing report: {ReportId}", request?.ReportId);

                    //Create PDF
                    var pdfBytes = _reportGeneratorService.GenerateReportPdf(request);

                    //Test Download PDF
                    //File.WriteAllBytes($"report_{request.ReportId}.pdf", pdfBytes);

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

}
