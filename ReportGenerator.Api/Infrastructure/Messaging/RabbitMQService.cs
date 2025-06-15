using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using ReportGenerator.Domain.Interfaces;
using ReportGenerator.Domain.Models;

namespace ReportGenerator.Api.Infrastructure.Messaging;

public interface IRabbitMQConnectionFactory
{
    IConnection CreateConnection();
}

public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory
{
    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };
        return factory.CreateConnection();
    }
}

public class RabbitMQService : IMessageQueueService, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName = "report_requests";

    public RabbitMQService(IRabbitMQConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task PublishReportRequestAsync(ReportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

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