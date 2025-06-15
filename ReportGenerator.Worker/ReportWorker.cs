using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReportGenerator.Worker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly string _queueName = "report_requests";
        private readonly IReportGeneratorService _reportGeneratorService;
        private readonly IWebhookService _webhookService;
        private readonly ILogger<ReportWorker> _logger;

        public ReportWorker(IReportGeneratorService reportGeneratorService, IWebhookService webhookService, ILogger<ReportWorker> logger)
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

                    //Test Download PDF
                    //File.WriteAllBytes($"./Reports/report_{request.ReportId}.pdf", pdfBytes);

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
            //_logger.LogInformation("Stopping ReportWorker...");

            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }

}
