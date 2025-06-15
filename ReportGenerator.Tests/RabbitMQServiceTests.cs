using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using ReportGenerator.Api.Infrastructure.Messaging;
using ReportGenerator.Domain.Models;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ReportGenerator.Tests;

public class RabbitMQServiceTests
{
    private readonly Mock<IRabbitMQConnectionFactory> _connectionFactoryMock;
    private readonly Mock<IConnection> _connectionMock;
    private readonly Mock<IModel> _channelMock;
    private readonly RabbitMQService _service;
    private readonly Mock<IOptions<RabbitMQSettings>> _settingsMock;

    public RabbitMQServiceTests()
    {
        _connectionFactoryMock = new Mock<IRabbitMQConnectionFactory>();
        _connectionMock = new Mock<IConnection>();
        _channelMock = new Mock<IModel>();
        _settingsMock = new Mock<IOptions<RabbitMQSettings>>();

        _connectionFactoryMock.Setup(f => f.CreateConnection()).Returns(_connectionMock.Object);
        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

        _settingsMock.Setup(s => s.Value).Returns(new RabbitMQSettings
        {
            HostName = TestConstants.RabbitMQHostTest,
            UserName = TestConstants.RabbitMQUserTest,
            Password = TestConstants.RabbitMQPasswordTest,
            QueueName = TestConstants.RabbitMQQueueTest
        });

        _service = new RabbitMQService(_connectionFactoryMock.Object, _settingsMock.Object);
    }

    [Fact]
    public async Task PublishReportRequestAsync_ValidRequest_PublishesMessage()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = TestConstants.ReportIdTest,
            WebhookUrl = TestConstants.UrlTest,
            Parameters = TestConstants.DictTest
        };
        var message = JsonSerializer.Serialize(request);
        var body = Encoding.UTF8.GetBytes(message);

        var propertiesMock = new Mock<IBasicProperties>();
        _channelMock.Setup(c => c.CreateBasicProperties()).Returns(propertiesMock.Object);

        // Act
        await _service.PublishReportRequestAsync(request);

        // Assert
        _channelMock.Verify(
            c => c.BasicPublish(
                It.Is<string>(exchange => exchange == string.Empty),
                It.Is<string>(routingKey => routingKey == TestConstants.RabbitMQQueueTest),
                It.Is<bool>(mandatory => mandatory == true),
                It.Is<IBasicProperties>(basicProperties => basicProperties == propertiesMock.Object),
                It.Is<ReadOnlyMemory<byte>>(b => b.ToArray().SequenceEqual(body))
            ),
            Times.Once()
        );
        propertiesMock.VerifySet(p => p.Persistent = true, Times.Once());
    }

    [Fact]
    public async Task PublishReportRequestAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        ReportRequest request = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.PublishReportRequestAsync(request));
    }
}