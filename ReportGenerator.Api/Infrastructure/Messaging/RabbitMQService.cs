using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using ReportGenerator.Domain.Interfaces;
using ReportGenerator.Domain.Models;

namespace ReportGenerator.Api.Infrastructure.Messaging;

public class RabbitMQService : IMessageQueueService, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName = "report_requests";

    public RabbitMQService()
    {
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

    public async Task PublishReportRequestAsync(ReportRequest request)
    {
        var message = JsonSerializer.Serialize(request);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(exchange: "", routingKey: _queueName, mandatory: true, basicProperties: properties, body: body);
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null && _channel.IsOpen)
            _channel.Close();
        if (_connection != null && _connection.IsOpen)
            _connection.Close();

        _channel?.Dispose();
        _connection?.Dispose();
        await Task.CompletedTask;
    }
}