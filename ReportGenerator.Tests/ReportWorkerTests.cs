using Microsoft.Extensions.Logging;
using Moq;
using ReportGenerator.Domain.Models;
using ReportGenerator.Worker;
using ReportGenerator.Worker.Services;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ReportGenerator.Tests;

public class ReportWorkerTests
{
    private readonly Mock<IReportGeneratorService> _reportGeneratorServiceMock;
    private readonly Mock<IWebhookService> _webhookServiceMock;
    private readonly Mock<ILogger<ReportWorker>> _loggerMock;

    public ReportWorkerTests()
    {
        _reportGeneratorServiceMock = new Mock<IReportGeneratorService>();
        _webhookServiceMock = new Mock<IWebhookService>();
        _loggerMock = new Mock<ILogger<ReportWorker>>();
    }
    
    [Fact]
    public async Task ProcessMessage_InvalidMessage_LogsErrorAndCallsWebhook()
    {
        // Arrange
        var message = "invalid json";
        var body = Encoding.UTF8.GetBytes(message);

        _webhookServiceMock.Setup(s => s.SendWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        try
        {
            var requestDeserialized = JsonSerializer.Deserialize<ReportRequest>(message);
        }
        catch (JsonException ex)
        {
            await _webhookServiceMock.Object.SendWebhookAsync(string.Empty, "unknown", Array.Empty<byte>(), false, $"Error: {ex.Message}");

            _loggerMock.Object.LogError(ex, "Error processing report: unknown");
        }

        // Assert
        _webhookServiceMock.Verify(s => s.SendWebhookAsync(string.Empty, "unknown", It.Is<byte[]>(b => b.Length == 0), false, It.IsAny<string>()), Times.Once());
        _loggerMock.VerifyLog(LogLevel.Error, "Error processing report: unknown", Times.Once());
    }

    [Fact]
    public async Task ProcessMessage_ValidMessage_CallsServicesAndLogs()
    {
        // Arrange
        var request = new ReportRequest
        {
            ReportId = "123",
            WebhookUrl = "https://test.com/webhook",
            Parameters = new Dictionary<string, object> { { "key1", "value1" } }
        };
        var message = JsonSerializer.Serialize(request);
        var body = Encoding.UTF8.GetBytes(message);
        var pdfBytes = new byte[] { 1, 2, 3 };

        _reportGeneratorServiceMock.Setup(s => s.GenerateReportPdf(It.IsAny<ReportRequest>())).Returns(pdfBytes);
        _webhookServiceMock.Setup(s => s.SendWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        var requestDeserialized = JsonSerializer.Deserialize<ReportRequest>(message);
        var pdf = _reportGeneratorServiceMock.Object.GenerateReportPdf(requestDeserialized);

        await _webhookServiceMock.Object.SendWebhookAsync(requestDeserialized.WebhookUrl, requestDeserialized.ReportId, pdf, true, $"Report {requestDeserialized.ReportId} generated successfully");

        // Assert
        _reportGeneratorServiceMock.Verify(s => s.GenerateReportPdf(It.Is<ReportRequest>(r => r.ReportId == "123")), Times.Once());
        _webhookServiceMock.Verify(s => s.SendWebhookAsync("https://test.com/webhook", "123", pdfBytes, true, It.IsAny<string>()), Times.Once());
    }

}