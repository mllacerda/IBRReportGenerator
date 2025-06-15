using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ReportGenerator.Domain.Interfaces;
using ReportGenerator.Domain.Models;
using System.Text;
using System.Text.Json;

namespace ReportGenerator.Api.Infrastructure.Messaging;

public interface IRabbitMQConnectionFactory
{
    IConnection CreateConnection();
}

public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory
{
    private readonly RabbitMQSettings _settings;

    public RabbitMQConnectionFactory(IOptions<RabbitMQSettings> settings)
    {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password
        };
        return factory.CreateConnection();
    }
}

public class RabbitMQService : IMessageQueueService, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;

    public RabbitMQService(IRabbitMQConnectionFactory connectionFactory, IOptions<RabbitMQSettings> settings)
    {
        var rabbitMQSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

        _queueName = rabbitMQSettings.QueueName;
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